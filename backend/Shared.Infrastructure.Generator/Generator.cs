using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Shared.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Shared.Infrastructure.Generator.Diagnostics;

namespace Shared.Infrastructure.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private const string Namespace = "Shared.Infrastructure.Generator";
    private const string GenerateApplySortAttributeFullName = $"{Namespace}.{nameof(GenerateApplySortAttribute)}";
    private const string SortExpressionAttributeFullName = $"{Namespace}.{nameof(SortExpressionAttribute)}";

    private static CompilationUnitSyntax GenerateApplySortAttribute()
    {
        var attributeUsage = AttributeList([
            Attribute(IdentifierName("AttributeUsage"))
                .AddArgumentListArguments(
                    AttributeArgument(IdentifierName("AttributeTargets").Member("Class")),
                    AttributeArgument(LiteralExpression(SyntaxKind.TrueLiteralExpression))
                        .WithNameEquals(NameEquals(IdentifierName("AllowMultiple")))
                )
        ]);

        var classDecl = ClassDeclaration(nameof(GenerateApplySortAttribute))
            .AddAttributeLists(attributeUsage)
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("PagedQueryType")).WithType(IdentifierName("Type")),
                Parameter(Identifier("EntityType")).WithType(IdentifierName("Type"))
            )
            .AddBaseListTypes(SimpleBaseType(IdentifierName("Attribute")))
            .WithoutBody();

        var namespaceDecl = FileScopedNamespaceDeclaration(ParseName(Namespace))
            .AddMembers(classDecl);

        var compilationUnit = CompilationUnit()
            .AddMembers(namespaceDecl)
            .ApplyGeneratorDefaults();

        return compilationUnit;
    }

    private static CompilationUnitSyntax SortExpressionAttribute()
    {
        var attributeUsage = AttributeList([
            Attribute(IdentifierName("AttributeUsage"))
                .AddArgumentListArguments(AttributeArgument(IdentifierName("AttributeTargets").Member("Field")))
        ]);

        var classDecl = ClassDeclaration(nameof(SortExpressionAttribute))
            .AddAttributeLists(attributeUsage)
            .AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword)
            )
            .AddTypeParameterListParameters(TypeParameter("T"))
            .AddParameterListParameters(Parameter(Identifier("SortField")).WithType(IdentifierName("T")))
            .AddBaseListTypes(SimpleBaseType(IdentifierName("Attribute")))
            .AddConstraintClauses(
                TypeParameterConstraintClause("T")
                    .AddConstraints(
                        TypeConstraint(IdentifierName("Enum"))
                    ))
            .WithoutBody();

        var namespaceDecl = FileScopedNamespaceDeclaration(ParseName(Namespace))
            .AddMembers(classDecl);

        var compilationUnit = CompilationUnit()
            .AddUsings(UsingDirective(IdentifierName("System")))
            .AddMembers(namespaceDecl)
            .ApplyGeneratorDefaults();

        return compilationUnit;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            var attribute = GenerateApplySortAttribute();
            postInitializationContext.AddSource(GenerateApplySortAttributeFullName + ".g.cs",
                SourceText.From(attribute.ToFullString(), Encoding.UTF8));
            attribute = SortExpressionAttribute();
            postInitializationContext.AddSource(SortExpressionAttributeFullName + ".g.cs",
                SourceText.From(attribute.ToFullString(), Encoding.UTF8));
        });

        var wellKnownTypesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return new WellKnownTypes(compilation);
            })
            .WithComparer(WellKnownTypesComparer.Instance);

        var generationData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateApplySortAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    List<GenerateApplySortData.AttributeArguments>? argumentsList = null;
                    foreach (var ctorArgs in ctx.Attributes.Select(attrData => attrData.ConstructorArguments)
                                 .Where(ctorArgs => ctorArgs.Length == 2))
                    {
                        if (ctorArgs[0].Value is not ITypeSymbol pagedQueryType ||
                            ctorArgs[1].Value is not ITypeSymbol entityType) continue;
                        (argumentsList ??= []).Add(
                            new GenerateApplySortData.AttributeArguments(pagedQueryType, entityType));
                    }

                    return argumentsList == null
                        ? null
                        : new GenerateApplySortData((INamedTypeSymbol)ctx.TargetSymbol, argumentsList);
                })
            .Where(static e => e is not null)
            .Select(static (e, _) => e!)
            .Combine(wellKnownTypesProvider);

        context.RegisterSourceOutput(generationData,
            static (spc, source) =>
            {
                try
                {
                    var (data, wellKnownTypes) = source;
                    Execute(data, wellKnownTypes, spc);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(InternalError, null,
                        exception.StackTrace.Replace("\r\n", "")));
                }
            });
    }

    private enum GenerationType
    {
        Single = 0,
        Multi = 1
    }
    
    private static void Execute(GenerateApplySortData generationData, WellKnownTypes wellKnownTypes,
        SourceProductionContext context)
    {
        var customExpressions = generationData.ClassSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Select(f => new
            {
                Field = f,
                Attr = f.GetAttributes()
                    .FirstOrDefault(a =>
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass?.OriginalDefinition,
                            wellKnownTypes.SortExpressionAttribute))
            })
            .Where(x => x.Attr is { ConstructorArguments.Length: 1 })
            .Select(x =>
            {
                var enumValue = x.Attr!.ConstructorArguments[0];
                var enumTypeSymbol = enumValue.Type;
                var enumFieldSymbol = enumTypeSymbol!.GetMembers()
                    .OfType<IFieldSymbol>()
                    .First(f => f.HasConstantValue && f.ConstantValue.Equals(enumValue.Value));

                return (EnumField: enumFieldSymbol, FieldName: x.Field.Name);
            })
            .ToDictionary(t => t.EnumField, t => t.FieldName, SymbolEqualityComparer.Default);

        List<MemberDeclarationSyntax> methods = [];
        Dictionary<string, MemberDeclarationSyntax> expressions = [];
        foreach (var args in generationData.Arguments)
        {
            GenerationType? generationType = null;
            for (var current = args.PagedQueryType; current is not null; current = current.OriginalDefinition.BaseType)
            {
                
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition,
                        wellKnownTypes.SingleSortPagedQuery))
                {
                    generationType = GenerationType.Single;
                    break;
                }

                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition,
                        wellKnownTypes.MultiSortPagedQuery))
                {
                    generationType = GenerationType.Multi;
                    break;
                }
            }

            if (generationType is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(InternalError, null,"generationType is null"));
                continue;
            }

            var sortTypeSymbol = wellKnownTypes.FindSortTypeEnum(args.PagedQueryType);

            if (sortTypeSymbol is null)
            {
                var fullName = args.PagedQueryType.ContainingNamespace.ToDisplayString();
                string className1 = args.PagedQueryType.Name;  
              context.ReportDiagnostic(Diagnostic.Create(InternalError, null,fullName +"."+className1 + "SortType"));
               
                continue;
            }

            var enumValues = sortTypeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .ToList();

            var properties = args.EntityType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                .ToDictionary(p => p.Name, p => p);
            
            var sortTypeName = sortTypeSymbol.GetGlobalName();
            var entityName = args.EntityType.GetGlobalName();

            List<SwitchExpressionArmSyntax> switchArms = [];
            foreach (var enumValue in enumValues)
            {
                if (!customExpressions.TryGetValue(enumValue, out var expressionName))
                {
                    if (!properties.TryGetValue(enumValue.Name, out var property)) continue;
                    expressionName = $"{args.EntityType.Name}_{property.Name}_Expression";

                    if (!expressions.ContainsKey(expressionName))
                    {
                        var fieldDeclaration =
                            FieldDeclaration(
                                    VariableDeclaration(
                                            GenericName("Expression")
                                                .AddTypeArgumentListArguments(
                                                    GenericName("Func")
                                                        .AddTypeArgumentListArguments(
                                                            entityName,
                                                            property.Type.GetGlobalName()
                                                        )
                                                )
                                        )
                                        .AddVariables(
                                            VariableDeclarator(expressionName)
                                                .WithInitializer(
                                                    EqualsValueClause(SimpleLambdaExpression(
                                                        Parameter(Identifier("e")),
                                                        IdentifierName("e").Member(property.Name)))
                                                )))
                                .AddModifiers(
                                    Token(SyntaxKind.PrivateKeyword),
                                    Token(SyntaxKind.StaticKeyword),
                                    Token(SyntaxKind.ReadOnlyKeyword)
                                );

                        expressions.Add(expressionName, fieldDeclaration);
                    }
                }

                var switchArm = generationType == GenerationType.Single
                    ? CreateSingleSortSwitchArm(sortTypeName, enumValue.Name, expressionName)
                    : CreateMultiSortSwitchArm(sortTypeName, enumValue.Name, expressionName);

                switchArms.Add(switchArm);
            }

            var assignmentStatement = ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("queryable"),
                    SwitchExpression(IdentifierName("sort").Member("Field"))
                        .WithArms(SeparatedList(switchArms))
                )
            );

            StatementSyntax[] statements;
            if (generationType == GenerationType.Single)
            {
                statements =
                [
                    CreateVariable(Identifier("sort"), IdentifierName("request").Member("Sort")),
                    assignmentStatement,
                    ReturnQueryable
                ];
            }
            else
            {
                var foreachStmt =
                    ForEachStatement(
                        IdentifierName("var"),
                        Identifier("sort"),
                        IdentifierName("request").Member("Sort"),
                        Block(assignmentStatement)
                            .AddStatements(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("isFirst"),
                                        LiteralExpression(SyntaxKind.FalseLiteralExpression)
                                    )
                                ))
                    );

                statements =
                [
                    CreateVariable(Identifier("isFirst"), LiteralExpression(SyntaxKind.TrueLiteralExpression)),
                    foreachStmt,
                    ReturnQueryable
                ];
            }

            var queryableGeneric = GenericName(Identifier("IQueryable"))
                .AddTypeArgumentListArguments(entityName);

            var requestSortType = GenericName(Identifier(generationType == GenerationType.Single
                    ? "IHasSingleSort"
                    : "IHasMultiSort"))
                .AddTypeArgumentListArguments(sortTypeName);

            var methodDecl = MethodDeclaration(queryableGeneric, "ApplySort")
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("queryable")).AddModifiers(Token(SyntaxKind.ThisKeyword))
                        .WithType(queryableGeneric),
                    Parameter(Identifier("request")).WithType(requestSortType)
                )
                .AddBodyStatements(statements);

            methods.Add(methodDecl);
        }

        var className = generationData.ClassSymbol.Name;

        var classDecl = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .AddMembers(expressions.Values.ToArray())
            .AddMembers(methods.ToArray());

        MemberDeclarationSyntax topLevelMember = classDecl;
        var ns = generationData.ClassSymbol.ContainingNamespace;
        if (!ns.IsGlobalNamespace)
        {
            var nsName = ParseName(ns.ToDisplayString());
            topLevelMember = FileScopedNamespaceDeclaration(nsName)
                .AddMembers(classDecl);
        }

        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System.Linq.Expressions")),
                UsingDirective(ParseName("Shared.Application.Enums")),
                UsingDirective(ParseName("Shared.Application.Interfaces")),
                UsingDirective(ParseName("static Shared.Infrastructure.Extensions.QueryableExtensions"))
            )
            .AddMembers(topLevelMember)
            .ApplyGeneratorDefaults();

        // foreach (var diagnostic in classContext.Diagnostics)
        // {
        //     context.ReportDiagnostic(diagnostic);
        // }
        // context.ReportDiagnostic(Diagnostic.Create(InternalError, null, ns.ToDisplayString()));

        context.AddSource($"{className}.GenerateApplySort.g.cs",
            SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
    }

    private static readonly ReturnStatementSyntax ReturnQueryable = ReturnStatement(IdentifierName("queryable"))
        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static SwitchExpressionArmSyntax CreateSingleSortSwitchArm(TypeSyntax sortTypeName, string enumValue,
        string expressionName) =>
        SwitchExpressionArm(
            ConstantPattern(sortTypeName.Member(enumValue)),
            ConditionalExpression(
                BinaryExpression(SyntaxKind.EqualsExpression,
                    IdentifierName("sort").Member("Order"),
                    IdentifierName("SortOrderType").Member("Ascending")
                ),
                InvocationExpression(IdentifierName("queryable").Member("OrderBy"))
                    .AddArgumentListArguments(Argument(IdentifierName(expressionName))),
                InvocationExpression(IdentifierName("queryable").Member("OrderByDescending"))
                    .AddArgumentListArguments(Argument(IdentifierName(expressionName)))
            )
        );

    private static SwitchExpressionArmSyntax CreateMultiSortSwitchArm(TypeSyntax sortTypeName, string enumValue,
        string expressionName) =>
        SwitchExpressionArm(
            ConstantPattern(sortTypeName.Member(enumValue)),
            InvocationExpression(IdentifierName("queryable").Member("ApplySort"))
                .AddArgumentListArguments(
                    Argument(IdentifierName(expressionName)),
                    Argument(IdentifierName("sort").Member("Order")),
                    Argument(IdentifierName("isFirst"))
                )
        );

    private static LocalDeclarationStatementSyntax CreateVariable(SyntaxToken identifier, ExpressionSyntax expression) =>
        LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator(identifier)
                        .WithInitializer(EqualsValueClause(expression))
                )
        );
}