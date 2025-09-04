using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharedKernel.Infrastructure.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class ApplySortGenerator : IIncrementalGenerator
{
    private const string AddApplySortAttributeMetadataName =
        "SharedKernel.Infrastructure.Generator.Attributes.AddApplySortAttribute";

    private enum GenerationType
    {
        Single = 0,
        Multi = 1
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDecl &&
               classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var name in classDeclaration.AttributeLists.SelectMany(attributeListSyntax =>
                     attributeListSyntax.Attributes.Select(attributeSyntax => attributeSyntax.Name.ToString())))
        {
            if (name is "AddApplySort" or "AddApplySortAttribute")
            {
                return classDeclaration;
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        var addApplySortAttributeSymbol = compilation.GetTypeByMetadataName(AddApplySortAttributeMetadataName);
        if (addApplySortAttributeSymbol == null)
        {
            return;
        }

        var distinctClasses = classes.Where(x => x is not null).Distinct();

        foreach (var classDeclaration in distinctClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration!.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null)
                continue;

            var applySortAttribute = GetAddApplySortAttribute(classSymbol, addApplySortAttributeSymbol);
            if (applySortAttribute == null)
                continue;

            var (enumType, entityType, generationType) = applySortAttribute.Value;
            var enumValues = GetEnumValues(enumType);

            var compilationUnit = GenerateApplySortExtension(classSymbol, enumType, entityType, enumValues, generationType);
            var source = compilationUnit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{classSymbol.Name}_ApplySort.g.cs", source);
        }
    }

    private static (INamedTypeSymbol enumType, INamedTypeSymbol entityType, GenerationType generationType)?
        GetAddApplySortAttribute(INamedTypeSymbol classSymbol, INamedTypeSymbol addApplySortAttributeSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, addApplySortAttributeSymbol)) continue;

            // support both old signature (2 args) and new (3 args)
            if (attribute.ConstructorArguments.Length < 2) continue;

            var enumTypeArg = attribute.ConstructorArguments[0];
            var entityTypeArg = attribute.ConstructorArguments[1];

            GenerationType generation = GenerationType.Multi; // default to previous behaviour (backwards compatible)

            if (attribute.ConstructorArguments.Length >= 3)
            {
                var thirdArg = attribute.ConstructorArguments[2].Value;
                try
                {
                    if (thirdArg is int intVal)
                    {
                        generation = (GenerationType)intVal;
                    }
                    else if (thirdArg is short s)
                    {
                        generation = (GenerationType)s;
                    }
                    else if (thirdArg is byte b)
                    {
                        generation = (GenerationType)b;
                    }
                    else if (thirdArg != null)
                    {
                        var name = thirdArg.ToString();
                        if (Enum.TryParse<GenerationType>(name, out var parsed))
                        {
                            generation = parsed;
                        }
                    }
                }
                catch
                {
                    generation = GenerationType.Multi;
                }
            }

            if (enumTypeArg.Value is INamedTypeSymbol enumType &&
                entityTypeArg.Value is INamedTypeSymbol entityType)
            {
                return (enumType, entityType, generation);
            }
        }

        return null;
    }

    private static (string Name, object Value)[] GetEnumValues(INamedTypeSymbol enumType)
    {
        return
        [
            .. enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(field => field.IsConst)
                .Select(field => (field.Name, field.ConstantValue ?? 0))
        ];
    }

    // Создаёт TypeSyntax с глобально-квалифицированным именем: "global::Namespace.Type"
    private static TypeSyntax CreateFullTypeSyntax(INamedTypeSymbol typeSymbol)
    {
        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        // Например: "global::CoreService.Domain.Entities.Thread"
        return ParseTypeName(fullName);
    }

    // Старая версия (оставил на всякий случай) — для случаев, где нужен простой QualifiedName
    private static TypeSyntax CreateTypeSyntax(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.ContainingType == null) return IdentifierName(typeSymbol.Name);
        var containingTypeSyntax = CreateTypeSyntax(typeSymbol.ContainingType);
        return QualifiedName((NameSyntax)containingTypeSyntax, IdentifierName(typeSymbol.Name));
    }

    private static CompilationUnitSyntax GenerateApplySortExtension(
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol enumType,
        INamedTypeSymbol entityType,
        (string Name, object Value)[] enumValues,
        GenerationType generationType)
    {
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : classSymbol.ContainingNamespace.ToDisplayString();

        var className = classSymbol.Name;

        // Собираем все необходимые using директивы
        var requiredUsings = new HashSet<string>
        {
            "SharedKernel.Application.Interfaces"
        };

        // Добавляем namespace для EnumType (учитываем что он может быть nested)
        var enumNamespace = enumType.ContainingType?.ContainingNamespace ?? enumType.ContainingNamespace;
        if (!enumNamespace.IsGlobalNamespace)
        {
            requiredUsings.Add(enumNamespace.ToDisplayString());
        }

        // Добавляем namespace для EntityType если он не в global namespace
        if (!entityType.ContainingNamespace.IsGlobalNamespace)
        {
            requiredUsings.Add(entityType.ContainingNamespace.ToDisplayString());
        }

        // Исключаем namespace самого класса чтобы избежать дублирования
        if (!string.IsNullOrEmpty(namespaceName))
        {
            requiredUsings.Remove(namespaceName);
        }

        // Если Single - потребуется SharedKernel.Application.Enums и System.Linq
        if (generationType == GenerationType.Single)
        {
            requiredUsings.Add("SharedKernel.Application.Enums");
            requiredUsings.Add("System.Linq");
        }
        else
        {
            // при Multi мы используем ApplySort extension (в вашем примере это static SharedKernel.Infrastructure.Extensions.QueryableExtensions)
            requiredUsings.Add("static SharedKernel.Infrastructure.Extensions.QueryableExtensions");
        }

        // Создаем using директивы из отсортированного списка
        var usingDirectives = requiredUsings
            .OrderBy(u => u)
            .Select(CreateUsingDirective)
            .ToArray();

        if (generationType == GenerationType.Multi)
        {
            // --- Multi: foreach + queryable.ApplySort(...)
            var switchArms = enumValues.Select(enumValue =>
                SwitchExpressionArm(
                    ConstantPattern(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateFullTypeSyntax(enumType),
                            IdentifierName(enumValue.Name))),
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("queryable"),
                                IdentifierName("ApplySort")))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>([
                                    Argument(IdentifierName($"{enumValue.Name}Expression")),
                                    Argument(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("sortCriteria"),
                                            IdentifierName("Order"))),
                                    Argument(IdentifierName("isFirst"))
                                ]))))).ToArray();

            var isFirstDeclaration = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("isFirst"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(SyntaxKind.TrueLiteralExpression))))));

            var foreachBodyStatements = new StatementSyntax[]
            {
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("queryable"),
                        SwitchExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("sortCriteria"),
                                    IdentifierName("Field")))
                            .WithArms(SeparatedList(switchArms)))),
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("isFirst"),
                        LiteralExpression(SyntaxKind.FalseLiteralExpression)))
            };

            var foreachStatement = ForEachStatement(
                IdentifierName("var"),
                Identifier("sortCriteria"),
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("request"),
                    IdentifierName("Sort")),
                Block(SyntaxFactory.List(foreachBodyStatements)));

            var returnStatement = ReturnStatement(IdentifierName("queryable"));

            var applySortMethod = MethodDeclaration(
                    GenericName(Identifier("IQueryable"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    CreateFullTypeSyntax(entityType)))),
                    Identifier("ApplySort"))
                .WithModifiers(
                    TokenList([
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)
                    ]))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>([
                            Parameter(Identifier("queryable"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(Identifier("IQueryable"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    CreateFullTypeSyntax(entityType))))),
                            Parameter(Identifier("request"))
                                .WithType(
                                    GenericName(Identifier("IHasMultiSort"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(
                                                    CreateFullTypeSyntax(enumType)))))
                        ])))
                .WithBody(
                    Block(
                        SyntaxFactory.List<StatementSyntax>([
                            isFirstDeclaration,
                            foreachStatement,
                            returnStatement
                        ])));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(
                    TokenList([
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)
                    ]))
                .WithMembers(List<MemberDeclarationSyntax>([
                    applySortMethod
                ]));

            var compilationUnit = CompilationUnit()
                .WithUsings(List(usingDirectives));

            if (string.IsNullOrEmpty(namespaceName))
            {
                compilationUnit = compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
            }
            else
            {
                var namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
                compilationUnit = compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
            }

            return compilationUnit;
        }
        else
        {
            // --- Single: IHasSingleSort<Enum> -> sort.Field switch with conditional OrderBy / OrderByDescending
            var sortLocal = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("sort"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("request"),
                                            IdentifierName("Sort")))))));

            var switchArms = enumValues.Select(enumValue =>
            {
                var condition = BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("sort"),
                        IdentifierName("Order")),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("SortOrderType"),
                        IdentifierName("Ascending")));

                var whenTrue = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("queryable"),
                        IdentifierName("OrderBy")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(Argument(IdentifierName($"{enumValue.Name}Expression")))));

                var whenFalse = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("queryable"),
                        IdentifierName("OrderByDescending")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(Argument(IdentifierName($"{enumValue.Name}Expression")))));

                var conditional = ConditionalExpression(condition, whenTrue, whenFalse);

                return SwitchExpressionArm(
                    ConstantPattern(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateFullTypeSyntax(enumType),
                            IdentifierName(enumValue.Name))),
                    conditional);
            }).ToArray();

            var assignQueryable = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("queryable"),
                    SwitchExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("sort"),
                            IdentifierName("Field")))
                        .WithArms(SeparatedList(switchArms))));

            var returnStatement = ReturnStatement(IdentifierName("queryable"));

            var applySortMethod = MethodDeclaration(
                    GenericName(Identifier("IQueryable"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    CreateFullTypeSyntax(entityType)))),
                    Identifier("ApplySort"))
                .WithModifiers(
                    TokenList([
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)
                    ]))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>([
                            Parameter(Identifier("queryable"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(Identifier("IQueryable"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    CreateFullTypeSyntax(entityType))))),
                            Parameter(Identifier("request"))
                                .WithType(
                                    GenericName(Identifier("IHasSingleSort"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(
                                                    CreateFullTypeSyntax(enumType)))))
                        ])))
                .WithBody(
                    Block(
                        SyntaxFactory.List<StatementSyntax>([
                            sortLocal,
                            assignQueryable,
                            returnStatement
                        ])));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(
                    TokenList([
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)
                    ]))
                .WithMembers(List<MemberDeclarationSyntax>([
                    applySortMethod
                ]));

            var compilationUnit = CompilationUnit()
                .WithUsings(List(usingDirectives));

            if (string.IsNullOrEmpty(namespaceName))
            {
                compilationUnit = compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
            }
            else
            {
                var namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
                compilationUnit = compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
            }

            return compilationUnit;
        }
    }

    private static UsingDirectiveSyntax CreateUsingDirective(string namespaceName)
    {
        var nameParts = namespaceName.Split('.');

        if (nameParts.Length == 1)
        {
            return UsingDirective(IdentifierName(nameParts[0]));
        }

        var qualifiedName = QualifiedName(IdentifierName(nameParts[0]), IdentifierName(nameParts[1]));

        for (var i = 2; i < nameParts.Length; i++)
        {
            qualifiedName = QualifiedName(qualifiedName, IdentifierName(nameParts[i]));
        }

        return UsingDirective(qualifiedName);
    }
}
