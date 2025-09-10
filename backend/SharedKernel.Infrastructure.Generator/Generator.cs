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
public sealed class Generator : IIncrementalGenerator
{
    private const string Namespace = "SharedKernel.Infrastructure.Generator";
    private const string GenerateApplySortAttributeMetadataName = $"{Namespace}.{nameof(GenerateApplySortAttribute)}";
    
    private enum GenerationType
    {
        Single = 0,
        Multi = 1
    }

    private static CompilationUnitSyntax GenerateApplySortAttribute()
    {
        var attributeUsage = AttributeList(
            SingletonSeparatedList(
                Attribute(IdentifierName("AttributeUsage"))
                    .WithArgumentList(
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("AttributeTargets"),
                                        IdentifierName("Class")
                                    )
                                )
                            )
                        )
                    )
            )
        );

        var classDecl = ClassDeclaration(nameof(GenerateApplySortAttribute))
            .WithAttributeLists(SingletonList(attributeUsage))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(IdentifierName("Attribute"))
                    )
                )
            );

        var ctor = ConstructorDeclaration(nameof(GenerateApplySortAttribute))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(
                ParameterList(
                    SeparatedList([
                        Parameter(Identifier("pagedQueryType")).WithType(IdentifierName("Type")),
                        Parameter(Identifier("entityType")).WithType(IdentifierName("Type"))
                    ])
                )
            )
            .WithBody(
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName("PagedQueryType"),
                            IdentifierName("pagedQueryType")
                        )
                    ),
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName("EntityType"),
                            IdentifierName("entityType")
                        )
                    )
                )
            );

        var pagedQueryProp = PropertyDeclaration(IdentifierName("Type"), "PagedQueryType")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(
                AccessorList(
                    List([
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ])
                )
            );

        var entityProp = PropertyDeclaration(IdentifierName("Type"), "EntityType")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(
                AccessorList(
                    List([
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ])
                )
            );

        classDecl = classDecl.WithMembers(
            List<MemberDeclarationSyntax>([ctor, pagedQueryProp, entityProp])
        );

        var namespaceDecl = NamespaceDeclaration(ParseName(Namespace))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDecl));

        var compilationUnit = CompilationUnit()
            .WithUsings(List([UsingDirective(IdentifierName("System"))]))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDecl))
            .NormalizeWhitespace();

        return compilationUnit;
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            var compilationUnit = GenerateApplySortAttribute();
            postInitializationContext.AddSource($"{Namespace}.g.cs", compilationUnit.ToFullString());
        });

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
            if (name is "GenerateApplySort" or "GenerateApplySortAttribute")
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

        var generateApplySortAttributeSymbol =
            compilation.GetTypeByMetadataName(GenerateApplySortAttributeMetadataName);
        if (generateApplySortAttributeSymbol == null)
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

            var generateApplySortAttribute =
                GetGenerateApplySortAttribute(classSymbol, generateApplySortAttributeSymbol);
            if (generateApplySortAttribute == null)
                continue;

            var (enumType, entityType, generationType) = generateApplySortAttribute.Value;
            var enumValues = GetEnumValues(enumType);

            var compilationUnit =
                GenerateApplySortExtension(classSymbol, enumType, entityType, enumValues, generationType);
            var source = compilationUnit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{classSymbol.Name}_ApplySort.g.cs", source);
        }
    }
    
    private static (INamedTypeSymbol enumType, INamedTypeSymbol entityType, GenerationType generationType)?
        GetGenerateApplySortAttribute(INamedTypeSymbol classSymbol, INamedTypeSymbol addApplySortAttributeSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, addApplySortAttributeSymbol)) continue;

            // нужно минимум 2 аргумента в конструкторе атрибута
            if (attribute.ConstructorArguments.Length < 2) continue;

            var firstArg = attribute.ConstructorArguments[0];
            var secondArg = attribute.ConstructorArguments[1];

            // Оба аргумента должны быть типами
            if (firstArg.Value is not INamedTypeSymbol firstArgType ||
                secondArg.Value is not INamedTypeSymbol entityType)
                continue;

            // Сценарий 1 (старый) — первый аргумент это enum (EnumType, EntityType [, SortGenerationType])
            if (firstArgType.TypeKind == TypeKind.Enum)
            {
                var enumType = firstArgType;
                // восстановим GenerationType из третьего аргумента, если есть (обратно-совместимо)
                var generation = GenerationType.Multi; // default (как было раньше)
                if (attribute.ConstructorArguments.Length >= 3)
                {
                    var thirdArg = attribute.ConstructorArguments[2].Value;
                    try
                    {
                        switch (thirdArg)
                        {
                            case int intVal:
                                generation = (GenerationType)intVal;
                                break;
                            case short s:
                                generation = (GenerationType)s;
                                break;
                            case byte b:
                                generation = (GenerationType)b;
                                break;
                            default:
                            {
                                if (thirdArg != null)
                                {
                                    var name = thirdArg.ToString();
                                    if (Enum.TryParse<GenerationType>(name, out var parsed))
                                    {
                                        generation = parsed;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch
                    {
                        generation = GenerationType.Multi;
                    }
                }

                return (enumType, entityType, generation);
            }

            // Сценарий 2 (новый) — первый аргумент это PagedQueryType -> нужно разрешить TEnum и тип Single/Multi

            var resolved = ResolvePagedQueryEnumAndGeneration(firstArgType);
            if (resolved.enumType != null)
            {
                return (resolved.enumType, entityType, resolved.generation);
            }

            // не удалось понять из PagedQueryType — пропускаем (без генерации)
        }

        return null;
    }

    private static (INamedTypeSymbol? enumType, GenerationType generation) ResolvePagedQueryEnumAndGeneration(
        INamedTypeSymbol pagedQueryType)
    {
        // Проверяем цепочку базовых типов
        var current = pagedQueryType;
        while (current != null)
        {
            if (current.IsGenericType)
            {
                var def = current.ConstructedFrom;
                if (IsGenericDefinitionName(def, "SingleSortPagedQuery") && current.TypeArguments.Length >= 1)
                {
                    switch (current.TypeArguments[0])
                    {
                        case INamedTypeSymbol { TypeKind: TypeKind.Enum } enumTypeSymbol:
                            return (enumTypeSymbol, GenerationType.Single);
                        case INamedTypeSymbol nts:
                            return (nts, GenerationType.Single);
                    }
                }

                if (IsGenericDefinitionName(def, "MultiSortPagedQuery") && current.TypeArguments.Length >= 1)
                {
                    switch (current.TypeArguments[0])
                    {
                        case INamedTypeSymbol { TypeKind: TypeKind.Enum } enumTypeSymbol:
                            return (enumTypeSymbol, GenerationType.Multi);
                        case INamedTypeSymbol nts:
                            return (nts, GenerationType.Multi);
                    }
                }
            }

            current = current.BaseType;
        }

        // Также проверяем интерфейсы (на случай реализации через интерфейс)
        foreach (var typeSymbol in pagedQueryType.AllInterfaces)
        {
            if (!typeSymbol.IsGenericType) continue;
            var def = typeSymbol.ConstructedFrom;
            if (IsGenericDefinitionName(def, "SingleSortPagedQuery") && typeSymbol.TypeArguments.Length >= 1)
            {
                switch (typeSymbol.TypeArguments[0])
                {
                    case INamedTypeSymbol { TypeKind: TypeKind.Enum } enumTypeSymbol:
                        return (enumTypeSymbol, GenerationType.Single);
                    case INamedTypeSymbol nts:
                        return (nts, GenerationType.Single);
                }
            }

            if (IsGenericDefinitionName(def, "MultiSortPagedQuery") && typeSymbol.TypeArguments.Length >= 1)
            {
                switch (typeSymbol.TypeArguments[0])
                {
                    case INamedTypeSymbol { TypeKind: TypeKind.Enum } enumTypeSymbol:
                        return (enumTypeSymbol, GenerationType.Multi);
                    case INamedTypeSymbol nts:
                        return (nts, GenerationType.Multi);
                }
            }
        }

        // Фолбэк: если внутри PagedQueryType есть вложенный тип SortType и он enum, используем его (без точного знания Single/Multi).
        var nested = pagedQueryType.GetTypeMembers("SortType").FirstOrDefault();
        if (nested is { TypeKind: TypeKind.Enum } nestedEnum)
        {
            // Возвращаем enum, а GenerationType оставим Multi (консервативный выбор).
            return (nestedEnum, GenerationType.Multi);
        }

        return (null, GenerationType.Multi);

        // helper для сравнения по имени generic-определения (без арности)
        static bool IsGenericDefinitionName(INamedTypeSymbol def, string expectedNameWithoutArity)
        {
            var name = def.Name;
            var backtickIndex = name.IndexOf('`');
            var baseName = backtickIndex >= 0 ? name.Substring(0, backtickIndex) : name;
            return string.Equals(baseName, expectedNameWithoutArity, StringComparison.Ordinal);
        }
    }

    private static (string Name, object Value)[] GetEnumValues(INamedTypeSymbol enumType)
    {
        return enumType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => field.IsConst)
            .Select(field => (field.Name, field.ConstantValue ?? 0))
            .ToArray();
    }

    // Создаёт TypeSyntax с глобально-квалифицированным именем: "global::Namespace.Type"
    private static TypeSyntax CreateFullTypeSyntax(INamedTypeSymbol typeSymbol)
    {
        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        // Например: "global::CoreService.Domain.Entities.Thread"
        return ParseTypeName(fullName);
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
                                SingletonSeparatedList(
                                    CreateFullTypeSyntax(entityType)))),
                    Identifier("ApplySort"))
                .WithModifiers(
                    TokenList(new[]
                    {
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)
                    }))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>([
                            Parameter(Identifier("queryable"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(Identifier("IQueryable"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(
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
                        List<StatementSyntax>([
                            isFirstDeclaration,
                            foreachStatement,
                            returnStatement
                        ])));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(
                    TokenList(new[]
                    {
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)
                    }))
                .WithMembers(List(new MemberDeclarationSyntax[] { applySortMethod }));

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
                compilationUnit =
                    compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
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
                                SingletonSeparatedList(
                                    CreateFullTypeSyntax(entityType)))),
                    Identifier("ApplySort"))
                .WithModifiers(
                    TokenList(new[]
                    {
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)
                    }))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>([
                            Parameter(Identifier("queryable"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(Identifier("IQueryable"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(
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
                        List<StatementSyntax>([
                            sortLocal,
                            assignQueryable,
                            returnStatement
                        ])));

            var classDeclaration = ClassDeclaration(className)
                .WithModifiers(
                    TokenList(new[]
                    {
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)
                    }))
                .WithMembers(List(new MemberDeclarationSyntax[] { applySortMethod }));

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
                compilationUnit =
                    compilationUnit.WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
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

        NameSyntax qualifiedName = IdentifierName(nameParts[0]);
        for (var i = 1; i < nameParts.Length; i++)
        {
            qualifiedName = QualifiedName(qualifiedName, IdentifierName(nameParts[i]));
        }

        return UsingDirective(qualifiedName);
    }
}