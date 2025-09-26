using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Shared.Generator;
using Shared.Presentation.Generator.Enums;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Shared.Presentation.Generator.Diagnostics;

namespace Shared.Presentation.Generator;

[Generator(LanguageNames.CSharp)]
public sealed partial class Generator : IIncrementalGenerator
{
    private const string Namespace = "Shared.Presentation.Generator";
    private const string GenerateBindAttributeFullName = Namespace + "." + nameof(GenerateBindAttribute);

    private static CompilationUnitSyntax GenerateBindAttribute()
    {
        var attributeDecl = ClassDeclaration(nameof(GenerateBindAttribute))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(SimpleBaseType(IdentifierName("Attribute")))
            .AddAttributeLists(
                AttributeList([
                    Attribute(IdentifierName("AttributeUsage"))
                        .AddArgumentListArguments(
                            AttributeArgument(IdentifierName("AttributeTargets").Member("Class")))
                ])
            )
            .WithoutBody();

        var namespaceDecl = FileScopedNamespaceDeclaration(ParseName(Namespace))
            .AddMembers(attributeDecl);

        var compilationUnit = CompilationUnit()
            .AddMembers(namespaceDecl)
            .ApplyGeneratorDefaults();

        return compilationUnit;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            var attr = GenerateBindAttribute();
            postInitializationContext.AddSource(GenerateBindAttributeFullName + ".g.cs",
                SourceText.From(attr.ToFullString(), Encoding.UTF8));
        });

        var classSymbols = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateBindAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol);

        var wellKnownTypesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return new WellKnownTypes(compilation);
            })
            .WithComparer(WellKnownTypesComparer.Instance);


        var classSymbolsWithWellKnownTypes = classSymbols
            .Combine(wellKnownTypesProvider);

        context.RegisterSourceOutput(classSymbolsWithWellKnownTypes,
            static (spc, source) =>
            {
                try
                {
                    var (classSymbol, wellKnownTypes) = source;
                    Execute(classSymbol, wellKnownTypes, spc);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(InternalError, null, exception.Message));
                }
            });
    }

    private static void Execute(INamedTypeSymbol classSymbol, WellKnownTypes wellKnownTypes,
        SourceProductionContext context)
    {
        var className = classSymbol.Name;
        var nullableClassType = NullableType(IdentifierName(className));

        var stringType = PredefinedType(Token(SyntaxKind.StringKeyword));
        var errorsDecl = LocalDeclarationStatement(
            VariableDeclaration(
                    GenericName(Identifier("Dictionary")).AddTypeArgumentListArguments(stringType, stringType)
                )
                .AddVariables(
                    VariableDeclarator(Identifier("errors")).WithInitializer(EqualsValueClause(CollectionExpression()))
                )
        );

        List<StatementSyntax> statements = [errorsDecl];

        var classContext = new ClassContext(classSymbol, wellKnownTypes);

        var hasFromBody = false;

        foreach (var property in classContext.Properties)
        {
            var declaringSyntaxRef = property.DeclaringSyntaxReferences.FirstOrDefault();
            var loc = declaringSyntaxRef?.GetSyntax().GetLocation() ?? Location.None;
            var propSyntax = declaringSyntaxRef?.GetSyntax() as PropertyDeclarationSyntax;

            if (propSyntax?.Initializer != null)
                classContext.Diagnostics.Add(Diagnostic.Create(InitializerNotAllowed, loc, property.Name));

            if (propSyntax?.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)) is not true)
                classContext.Diagnostics.Add(Diagnostic.Create(PropertyMustBeRequired, loc, property.Name));

            var hasGetAccessor = false;
            var hasInitAccessor = false;
            if (propSyntax?.AccessorList != null)
            {
                foreach (var accessor in propSyntax.AccessorList.Accessors)
                {
                    if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) hasGetAccessor = true;
                    if (accessor.IsKind(SyntaxKind.InitAccessorDeclaration)) hasInitAccessor = true;
                }
            }

            if (!hasGetAccessor || !hasInitAccessor)
                classContext.Diagnostics.Add(Diagnostic.Create(PropertyMustHaveGetInit, loc, property.Name));

            if (!classContext.TryGetBindingKind(property, loc, out var bindingKind)) continue;

            var name = property.Name.ToCamelCase();

            if (bindingKind == ParameterBinding.FromBody)
            {
                if (hasFromBody)
                {
                    classContext.Diagnostics.Add(Diagnostic.Create(MultipleFromBodyNotAllowed, loc, property.Name));
                    continue;
                }

                if (property.Name != "Body")
                {
                    classContext.Diagnostics.Add(Diagnostic.Create(FromBodyMustBeNamedBody, loc, property.Name));
                }

                var invocation = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("context"),
                            GenericName("GetRequiredBodyFromJsonAsync")
                                .AddTypeArgumentListArguments(property.Type.GetGlobalName()))
                    )
                    .AddArgumentListArguments(Argument(IdentifierName("errors")));

                var awaited = AwaitExpression(invocation);
                var localDecl = LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator(Identifier(name)).WithInitializer(EqualsValueClause(awaited))
                        )
                );
                statements.Add(localDecl);
                hasFromBody = true;
                continue;
            }

            var nullableUnderlyingType = property.TryGetUnderlyingType();
            var type = nullableUnderlyingType ?? property.Type;

            var defaultField = classContext.TryGetDefaultField(property);

            var parameterKind = wellKnownTypes.GetParameterKind(type, out var arg0, out var arg1);

            SeparatedSyntaxList<ArgumentSyntax> args =
            [
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name))),
                Argument(IdentifierName("errors"))
            ];

            ParameterBehavior parameterBehavior;
            if (nullableUnderlyingType == null)
            {
                if (defaultField == null)
                {
                    parameterBehavior = ParameterBehavior.Required;
                }
                else
                {
                    if (bindingKind == ParameterBinding.FromRoute)
                    {
                        classContext.Diagnostics.Add(Diagnostic.Create(FromRouteCannotHaveDefaultValue, loc,
                            property.Name));
                        continue;
                    }

                    args = args.Add(Argument(ParseName(defaultField)));
                    parameterBehavior = ParameterBehavior.Defaultable;
                }
            }
            else
            {
                if (defaultField == null)
                {
                    if (bindingKind == ParameterBinding.FromRoute)
                    {
                        classContext.Diagnostics.Add(Diagnostic.Create(FromRouteCannotBeNullable, loc,
                            property.Name));
                        continue;
                    }

                    parameterBehavior = ParameterBehavior.Optional;
                }
                else
                {
                    classContext.Diagnostics.Add(Diagnostic.Create(NullableWithDefaultValue, loc,
                        property.Name));
                    continue;
                }
            }

            SeparatedSyntaxList<TypeSyntax> arguments;
            switch (parameterKind)
            {
                case ParameterType.ValueObject:
                    arguments = [arg0!.GetGlobalName(), arg1!.GetGlobalName()];
                    break;
                case ParameterType.ValueType or ParameterType.ReferenceType or ParameterType.Enum
                    or ParameterType.Primitive:
                    arguments = [type.GetGlobalName()];
                    break;
                default:
                    continue;
            }

            string methodName;
            if (bindingKind == ParameterBinding.FromRoute)
            {
                methodName = $"TryParse{parameterKind:G}FromRoute";
            }
            else if (bindingKind == ParameterBinding.FromQuery)
            {
                methodName = $"TryParse{parameterBehavior:G}{parameterKind:G}FromQuery";
            }
            else continue;

            var methodCall = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("context"),
                        GenericName(methodName)
                            .WithTypeArgumentList(TypeArgumentList(arguments)))
                )
                .WithArgumentList(ArgumentList(args));

            var statement = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator(name).WithInitializer(EqualsValueClause(methodCall))
                    )
            );

            statements.Add(statement);
        }

        var throwIfErrors = IfStatement(
            BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                IdentifierName("errors").Member("Count"),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))
            ),
            ThrowStatement(
                ObjectCreationExpression(IdentifierName("ValidationException"))
                    .AddArgumentListArguments(
                        Argument(IdentifierName("errors"))
                    )
            )
        );

        statements.Add(throwIfErrors);

        var initializerExpressions = classContext.Properties
            .Select(pi =>
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(pi.Name),
                    IdentifierName(pi.Name.ToCamelCase())
                )
            )
            .ToArray();

        var newObj =
            ObjectCreationExpression(classSymbol.GetGlobalName())
                .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                    SeparatedList<ExpressionSyntax>(initializerExpressions)));

        if (hasFromBody)
        {
            statements.Add(ReturnStatement(newObj));
        }
        else
        {
            var genericFromResult = GenericName("FromResult").AddTypeArgumentListArguments(nullableClassType);

            var valueTaskFromResult = InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("ValueTask"),
                    genericFromResult
                ),
                ArgumentList([Argument(newObj)])
            );

            statements.Add(ReturnStatement(valueTaskFromResult));
        }

        var bodySyntaxList = List(statements);

        if (classContext.Diagnostics.Count > 0)
        {
            var throwStmt =
                ThrowStatement(
                    ObjectCreationExpression(IdentifierName("NotImplementedException"))
                        .AddArgumentListArguments(
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal("Cannot generate BindAsync")))
                        )
                );

            bodySyntaxList = bodySyntaxList.Insert(0, throwStmt);
        }

        var methodDecl =
            MethodDeclaration(
                    GenericName(Identifier("ValueTask")).AddTypeArgumentListArguments(nullableClassType),
                    "BindAsync"
                )
                .AddModifiers(hasFromBody
                    ? [Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.AsyncKeyword)]
                    : [Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)]
                )
                .AddParameterListParameters(
                    Parameter(Identifier("context")).WithType(ParseTypeName("HttpContext")),
                    Parameter(Identifier("parameter")).WithType(ParseTypeName("ParameterInfo"))
                )
                .WithBody(Block(bodySyntaxList));

        var classDecl = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .AddMembers(methodDecl);

        MemberDeclarationSyntax topLevelMember = classDecl;
        var ns = classSymbol.ContainingNamespace;
        if (!ns.IsGlobalNamespace)
        {
            var nsName = ParseName(ns.ToDisplayString());
            topLevelMember = FileScopedNamespaceDeclaration(nsName)
                .AddMembers(classDecl);
        }

        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System.Reflection")),
                UsingDirective(ParseName("Microsoft.AspNetCore.Http")),
                UsingDirective(ParseName("Shared.Presentation.Exceptions")),
                UsingDirective(ParseName("static Shared.Presentation.Extensions.HttpContextExtensions"))
            )
            .AddMembers(topLevelMember)
            .ApplyGeneratorDefaults();

        foreach (var diagnostic in classContext.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        context.AddSource($"{className}.g.cs", SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
    }
}