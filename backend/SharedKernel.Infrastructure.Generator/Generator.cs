using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[Generator(LanguageNames.CSharp)]
public class ApplySortGenerator : IIncrementalGenerator
{
    private const string AddApplySortAttributeMetadataName =
        "SharedKernel.Infrastructure.Generator.Attributes.AddApplySortAttribute";

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

            var (enumType, entityType) = applySortAttribute.Value;
            var enumValues = GetEnumValues(enumType);

            var compilationUnit = GenerateApplySortExtension(classSymbol, enumType, entityType, enumValues);
            var source = compilationUnit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{classSymbol.Name}_ApplySort.g.cs", source);
        }
    }

    private static (INamedTypeSymbol enumType, INamedTypeSymbol entityType)? GetAddApplySortAttribute(
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol addApplySortAttributeSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, addApplySortAttributeSymbol)) continue;
            if (attribute.ConstructorArguments.Length != 2) continue;
            var enumTypeArg = attribute.ConstructorArguments[0];
            var entityTypeArg = attribute.ConstructorArguments[1];

            if (enumTypeArg.Value is INamedTypeSymbol enumType &&
                entityTypeArg.Value is INamedTypeSymbol entityType)
            {
                return (enumType, entityType);
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
        (string Name, object Value)[] enumValues)
    {
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : classSymbol.ContainingNamespace.ToDisplayString();

        var className = classSymbol.Name;
        var entityTypeName = entityType.Name;

        // Собираем все необходимые using директивы
        var requiredUsings = new HashSet<string>
        {
            "System.Linq.Expressions",
            "SharedKernel.Application.Interfaces",
            "static SharedKernel.Infrastructure.Extensions.QueryableExtensions"
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

        // Создаем using директивы из отсортированного списка
        var usingDirectives = requiredUsings
            .OrderBy(u => u)
            .Select(CreateUsingDirective)
            .ToArray();

        // Switch arms - для каждого enum значения ожидаем соответствующий Expression
        var switchArms = enumValues.Select(enumValue =>
                SwitchExpressionArm(
                    ConstantPattern(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateTypeSyntax(enumType),
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
                                ])))))
            .ToArray();

        // ApplySort метод
        var applySortMethod = MethodDeclaration(
                GenericName(Identifier("IQueryable"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                IdentifierName(entityTypeName)))),
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
                                                IdentifierName(entityTypeName))))),
                        Parameter(Identifier("request"))
                            .WithType(
                                GenericName(Identifier("IHasSort"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList(
                                                CreateTypeSyntax(enumType)))))
                    ])))
            .WithBody(
                Block(
                    SyntaxFactory.List<StatementSyntax>([
                        IfStatement(
                                BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("request"),
                                        IdentifierName("Sort")),
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)),
                                Block(
                                    SyntaxFactory.List<StatementSyntax>([
                                        LocalDeclarationStatement(
                                            VariableDeclaration(
                                                    IdentifierName("var"))
                                                .WithVariables(
                                                    SingletonSeparatedList(
                                                        VariableDeclarator(Identifier("isFirst"))
                                                            .WithInitializer(
                                                                EqualsValueClause(
                                                                    LiteralExpression(SyntaxKind
                                                                        .TrueLiteralExpression)))))),
                                        ForEachStatement(
                                            IdentifierName("var"),
                                            Identifier("sortCriteria"),
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("request"),
                                                IdentifierName("Sort")),
                                            Block(
                                                SyntaxFactory.List<StatementSyntax>([
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            IdentifierName("queryable"),
                                                            SwitchExpression(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("sortCriteria"),
                                                                        IdentifierName("Field")))
                                                                .WithArms(
                                                                    SeparatedList(switchArms)))),
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            IdentifierName("isFirst"),
                                                            LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                                                ])))
                                    ])))
                            .WithElse(
                                ElseClause(
                                    Block(
                                        SyntaxFactory.List<StatementSyntax>([
                                            ExpressionStatement(
                                                AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    IdentifierName("queryable"),
                                                    InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("queryable"),
                                                                IdentifierName("OrderBy")))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(IdentifierName(
                                                                        $"{enumValues[0].Name}Expression")))))))
                                        ])))),
                        ReturnStatement(IdentifierName("queryable"))
                    ])));

        // Класс
        var classDeclaration = ClassDeclaration(className)
            .WithModifiers(
                TokenList([
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.PartialKeyword)
                ]))
            .WithMembers(List<MemberDeclarationSyntax>([
                applySortMethod
            ]));

        // Compilation unit
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