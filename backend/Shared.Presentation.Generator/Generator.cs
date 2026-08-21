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

public enum AuthorizeMode : byte
{
    None = 0,
    Required = 1,
    Optional = 2
}

[Generator(LanguageNames.CSharp)]
public sealed partial class Generator : IIncrementalGenerator
{
    private const string Namespace = "Shared.Presentation.Generator";
    private const string GenerateBindAttributeFullName = Namespace + ".Attributes.GenerateBindAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        InitializeEndpoints(context);
        InitializeResultJsonConverters(context);

        var classSymbols = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateBindAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                    // Получаем атрибут GenerateBindAttribute
                    var attribute = ctx.Attributes.FirstOrDefault();

                    // Извлекаем значение AuthorizeMode (первый аргумент конструктора)
                    var authorizeMode = AuthorizeMode.None; // значение по умолчанию
                    if (attribute?.ConstructorArguments.Length > 0)
                    {
                        var arg = attribute.ConstructorArguments[0];
                        if (arg.Value is byte value)
                        {
                            authorizeMode = (AuthorizeMode)value;
                        }
                    }

                    return (ClassSymbol: classSymbol, AuthorizeMode: authorizeMode);
                });

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
                    var ((classSymbol, authorizeMode), wellKnownTypes) = source;
                    Execute(classSymbol, authorizeMode, wellKnownTypes, spc);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(InternalError, null, exception.Message));
                }
            });
    }

    private static void Execute(INamedTypeSymbol classSymbol, AuthorizeMode authorizeMode,
        WellKnownTypes wellKnownTypes, SourceProductionContext context)
    {
        var className = classSymbol.Name;
        var nullableClassType = NullableType(IdentifierName(className));

        var stringType = PredefinedType(Token(SyntaxKind.StringKeyword));
        var errorsDecl = LocalDeclarationStatement(
            VariableDeclaration(
                    NullableType(
                        GenericName(Identifier("Dictionary"))
                            .AddTypeArgumentListArguments(stringType, stringType)))
                .AddVariables(
                    VariableDeclarator(Identifier("errors"))
                        .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                )
        );

        List<StatementSyntax> statements = [errorsDecl];

        var classContext = new ClassContext(classSymbol, wellKnownTypes);
        var bindingMetadata = new List<ExpressionSyntax>();

        var hasFromBody = false;
        ITypeSymbol? bodyType = null;

        if (authorizeMode is AuthorizeMode.Required or AuthorizeMode.Optional)
        {
            var methodName = authorizeMode == AuthorizeMode.Required
                ? "GetRequiredActor"
                : "GetOptionalActor";

            var invocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("context"),
                    IdentifierName(methodName)));

            var localDecl = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator(Identifier("requestedBy")).WithInitializer(EqualsValueClause(invocation))
                    )
            );
            statements.Add(localDecl);
        }

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

            var variableName = property.Name.ToCamelCase();
            var bindingName = GetBindingName(property) ?? variableName;

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

                var nullableBodyType = NullableType(property.Type.GetGlobalName());
                statements.Add(LocalDeclarationStatement(
                    VariableDeclaration(nullableBodyType)
                        .AddVariables(VariableDeclarator(Identifier(variableName)))));

                var readBodyInvocation = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("context"),
                        GenericName("ReadBodyFromJsonAsync")
                            .AddTypeArgumentListArguments(property.Type.GetGlobalName())));

                var assignBody = ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(variableName),
                        AwaitExpression(readBodyInvocation)));

                var addNullBodyError = IfStatement(
                    IsPatternExpression(
                        IdentifierName(variableName),
                        ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("AddInvalidJsonBodyError"))
                            .AddArgumentListArguments(
                                Argument(IdentifierName("errors"))
                                    .WithRefKindKeyword(Token(SyntaxKind.RefKeyword)))));

                var jsonExceptionCatch = CatchClause()
                    .WithDeclaration(
                        CatchDeclaration(ParseTypeName("global::System.Text.Json.JsonException"))
                            .WithIdentifier(Identifier("exception")))
                    .WithBlock(Block(
                        ExpressionStatement(
                            InvocationExpression(IdentifierName("AddJsonBodyError"))
                                .AddArgumentListArguments(
                                    Argument(IdentifierName("errors"))
                                        .WithRefKindKeyword(Token(SyntaxKind.RefKeyword)),
                                    Argument(IdentifierName("exception")))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(variableName),
                                LiteralExpression(SyntaxKind.DefaultLiteralExpression)))));

                var fallbackCatch = CatchClause()
                    .WithBlock(Block(
                        ExpressionStatement(
                            InvocationExpression(IdentifierName("AddInvalidJsonBodyError"))
                                .AddArgumentListArguments(
                                    Argument(IdentifierName("errors"))
                                        .WithRefKindKeyword(Token(SyntaxKind.RefKeyword)))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(variableName),
                                LiteralExpression(SyntaxKind.DefaultLiteralExpression)))));

                statements.Add(TryStatement(
                    Block(assignBody, addNullBodyError),
                    List([jsonExceptionCatch, fallbackCatch]),
                    null));
                hasFromBody = true;
                bodyType = property.Type;
                continue;
            }

            var nullableUnderlyingType = property.TryGetUnderlyingType();
            var type = nullableUnderlyingType ?? property.Type;

            var defaultField = classContext.TryGetDefaultField(property);

            var parameterKind = wellKnownTypes.GetParameterKind(type, out var arg0, out var arg1);

            SeparatedSyntaxList<ArgumentSyntax> args =
            [
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(bindingName))),
                Argument(IdentifierName("errors")).WithRefKindKeyword(Token(SyntaxKind.RefKeyword))
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
                        VariableDeclarator(variableName).WithInitializer(EqualsValueClause(methodCall))
                    )
            );

            statements.Add(statement);
            bindingMetadata.Add(CreateBindingMetadata(
                type,
                bindingKind,
                bindingName,
                nullableUnderlyingType is not null,
                defaultField));
        }

        var throwIfErrors = IfStatement(
            IsPatternExpression(
                IdentifierName("errors"),
                UnaryPattern(ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression)))),
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
            {
                ExpressionSyntax value = IdentifierName(pi.Name.ToCamelCase());
                if (hasFromBody && pi.Name == "Body")
                {
                    value = PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, value);
                }

                return AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(pi.Name),
                    value);
            })
            .ToList();

        if (authorizeMode is AuthorizeMode.Required or AuthorizeMode.Optional)
        {
            initializerExpressions.Add(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("RequestedBy"),
                    IdentifierName("requestedBy")
                )
            );
        }

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
            .AddBaseListTypes(
                SimpleBaseType(
                    GenericName("IBindableFromHttpContext")
                        .AddTypeArgumentListArguments(IdentifierName(className))),
                SimpleBaseType(ParseTypeName("IEndpointParameterMetadataProvider")))
            .AddMembers(methodDecl, CreateEndpointParameterMetadataMethod(bindingMetadata, bodyType));

        if (authorizeMode is AuthorizeMode.Required or AuthorizeMode.Optional)
        {
            var propertyType = authorizeMode == AuthorizeMode.Required
                ? (TypeSyntax)ParseTypeName("ActorContext")
                : NullableType(ParseTypeName("ActorContext"));

            var requestedByProperty = PropertyDeclaration(propertyType, Identifier("RequestedBy"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.RequiredKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                );

            // Создаём тело метода PopulateMetadata
            var populateMetadataStatements = new List<StatementSyntax>
            {
                // builder.Metadata.Add(new AuthorizeAttribute());
                ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("builder"),
                                    IdentifierName("Metadata")),
                                IdentifierName("Add")))
                        .AddArgumentListArguments(
                            Argument(
                                ObjectCreationExpression(IdentifierName("AuthorizeAttribute"))
                                    .WithArgumentList(
                                        authorizeMode == AuthorizeMode.Required
                                            ? ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(ParseExpression(
                                                        "global::Shared.Presentation.Authorization.AuthenticationPolicies.PublicApi"))))
                                            : ArgumentList()))))
            };

            if (authorizeMode == AuthorizeMode.Required)
                populateMetadataStatements.Add(CreateUnauthorizedResponseMetadata(
                    "global::Shared.Presentation.Errors.AuthenticationRequiredError"));

            populateMetadataStatements.Add(CreateUnauthorizedResponseMetadata(
                "global::Shared.Presentation.Errors.ClaimNotFoundError"));

            // Для Optional добавляем AllowAnonymousAttribute
            if (authorizeMode == AuthorizeMode.Optional)
            {
                populateMetadataStatements.Add(
                    // builder.Metadata.Add(new AllowAnonymousAttribute());
                    ExpressionStatement(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("builder"),
                                        IdentifierName("Metadata")),
                                    IdentifierName("Add")))
                            .AddArgumentListArguments(
                                Argument(
                                    ObjectCreationExpression(IdentifierName("AllowAnonymousAttribute"))
                                        .WithArgumentList(ArgumentList())))));
            }

            var populateMetadataMethod = MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    "PopulateMetadata")
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("method")).WithType(ParseTypeName("MethodInfo")),
                    Parameter(Identifier("builder")).WithType(ParseTypeName("EndpointBuilder")))
                .WithBody(Block(populateMetadataStatements));

            classDecl = classDecl
                .AddBaseListTypes(SimpleBaseType(ParseTypeName("IEndpointMetadataProvider")))
                .AddMembers(requestedByProperty, populateMetadataMethod);
        }

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
                UsingDirective(ParseName("System.Collections.Generic")),
                UsingDirective(ParseName("Microsoft.AspNetCore.Http")),
                UsingDirective(ParseName("Microsoft.AspNetCore.Authorization")),
                UsingDirective(ParseName("Microsoft.AspNetCore.Http.Metadata")),
                UsingDirective(ParseName("Shared.Presentation.Exceptions")),
                UsingDirective(ParseName("Shared.Presentation.Binding")),
                UsingDirective(ParseName("Shared.Domain.ValueObjects")),
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

    private static string? GetBindingName(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.ToDisplayString();
            if (attributeName is not ("Microsoft.AspNetCore.Mvc.FromRouteAttribute" or
                "Microsoft.AspNetCore.Mvc.FromQueryAttribute"))
                continue;

            foreach (var argument in attribute.NamedArguments)
                if (argument is { Key: "Name", Value.Value: string name } &&
                    !string.IsNullOrWhiteSpace(name))
                    return name;
        }

        return null;
    }

    private static ExpressionSyntax CreateBindingMetadata(
        ITypeSymbol parameterType,
        ParameterBinding binding,
        string name,
        bool isNullable,
        string? defaultField)
    {
        var source = binding switch
        {
            ParameterBinding.FromRoute => "Path",
            ParameterBinding.FromQuery => "Query",
            _ => throw new InvalidOperationException($"Binding {binding} is not an OpenAPI parameter")
        };
        var hasDefault = defaultField is not null;

        return ObjectCreationExpression(IdentifierName("GeneratedRequestParameterMetadata"))
            .AddArgumentListArguments(
                Argument(TypeOfExpression(parameterType.GetGlobalName())),
                Argument(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("GeneratedRequestParameterSource"),
                    IdentifierName(source))),
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name))),
                Argument(LiteralExpression(isNullable
                    ? SyntaxKind.TrueLiteralExpression
                    : SyntaxKind.FalseLiteralExpression)),
                Argument(LiteralExpression(hasDefault
                    ? SyntaxKind.TrueLiteralExpression
                    : SyntaxKind.FalseLiteralExpression)),
                Argument(defaultField is null
                    ? LiteralExpression(SyntaxKind.NullLiteralExpression)
                    : ParseName(defaultField)));
    }

    private static MethodDeclarationSyntax CreateEndpointParameterMetadataMethod(
        IReadOnlyCollection<ExpressionSyntax> bindingMetadata,
        ITypeSymbol? bodyType)
    {
        var statements = new List<StatementSyntax>();
        if (bindingMetadata.Count > 0)
        {
            var parameters = CollectionExpression(
                SeparatedList<CollectionElementSyntax>(bindingMetadata.Select(ExpressionElement)));
            statements.Add(AddEndpointMetadata(
                ObjectCreationExpression(IdentifierName("GeneratedRequestBindingMetadata"))
                    .AddArgumentListArguments(Argument(parameters))));
        }

        if (bodyType is not null)
        {
            var contentTypes = CollectionExpression(
                SeparatedList<CollectionElementSyntax>(
                [
                    ExpressionElement(
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("application/json")))
                ]));
            statements.Add(AddEndpointMetadata(
                ObjectCreationExpression(IdentifierName("AcceptsMetadata"))
                    .AddArgumentListArguments(
                        Argument(contentTypes),
                        Argument(TypeOfExpression(bodyType.GetGlobalName())),
                        Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))));
        }

        return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                "PopulateMetadata")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("parameter")).WithType(ParseTypeName("ParameterInfo")),
                Parameter(Identifier("builder")).WithType(ParseTypeName("EndpointBuilder")))
            .WithBody(Block(statements));
    }

    private static StatementSyntax CreateUnauthorizedResponseMetadata(string errorType) =>
        AddEndpointMetadata(
            ObjectCreationExpression(IdentifierName("ProducesResponseTypeMetadata"))
                .AddArgumentListArguments(
                    Argument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("StatusCodes"),
                        IdentifierName("Status401Unauthorized"))),
                    Argument(TypeOfExpression(ParseTypeName(errorType)))));

    private static StatementSyntax AddEndpointMetadata(ExpressionSyntax metadata) =>
        ExpressionStatement(
            InvocationExpression(
                    IdentifierName("builder")
                        .Member("Metadata")
                        .Member("Add"))
                .AddArgumentListArguments(Argument(metadata)));
}
