using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharedKernel.Presentation.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor MissingBindingAttribute = new(
        id: "GB0001",
        title: "Property must have binding attribute",
        messageFormat: "Property '{0}' must have one of [FromRoute], [FromQuery], [FromBody]",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InitializerNotAllowed = new(
        id: "GB0002",
        title: "Initializer not allowed in [GenerateBind] class",
        messageFormat:
        "Property '{0}' must not have an initializer in a type annotated with [GenerateBind]. Move default values to the nested Defaults class.",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PropertyMustBeRequired = new(
        id: "GB0003",
        title: "Property must be declared 'required'",
        messageFormat: "Property '{0}' must be declared with the 'required' modifier in a [GenerateBind] type.",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PropertyMustHaveGetInit = new(
        id: "GB0004",
        title: "Property must have 'get; init;' accessors",
        messageFormat: "Property '{0}' must declare accessors 'get; init;' (auto-property) in a [GenerateBind] type.",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DefaultsMemberNotFound = new(
        id: "GB0005",
        title: "Defaults contains member with no matching property",
        messageFormat:
        "Defaults contains member '{0}' but no matching public property '{0}' exists in the enclosing [GenerateBind] type.",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext ctx)
    {
        ctx.RegisterPostInitializationOutput(static ctx2 =>
        {
            var attr = CreateGenerateBindAttribute();
            ctx2.AddSource("GenerateBindAttribute.g.cs", attr.ToFullString());
        });

        var classes = ctx.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx2, _) => (ClassDeclarationSyntax)ctx2.Node)
            .Where(static n => n is not null);

        var compilationAndClasses = ctx.CompilationProvider.Combine(classes.Collect());

        ctx.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
        {
            var compilation = source.Left;
            var classNodes = source.Right;
            if (classNodes.IsDefaultOrEmpty) return;

            foreach (var classNode in classNodes)
            {
                if (classNode is null) continue;
                var model = compilation.GetSemanticModel(classNode.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(classNode);
                if (symbol is null) continue;

                if (!HasGenerateBindAttribute(symbol)) continue;

                var diagnostics = new List<Diagnostic>();
                var generated = GenerateForClass(symbol, diagnostics);

                foreach (var d in diagnostics) spc.ReportDiagnostic(d);

                if (generated == null) continue;
                var hint = $"{symbol.Name}.GenerateBind.g.cs";
                spc.AddSource(hint, generated);
            }
        });
    }

    private static CompilationUnitSyntax CreateGenerateBindAttribute()
    {
        var attrClass = ClassDeclaration("GenerateBindAttribute")
            .WithModifiers(TokenList(
                Token(SyntaxKind.InternalKeyword),
                Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(ParseTypeName("System.Attribute")))))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));

        // Построение SharedKernel.Presentation.Generator
        NameSyntax nsName =
            QualifiedName(
                QualifiedName(
                    IdentifierName("SharedKernel"),
                    IdentifierName("Presentation")),
                IdentifierName("Generator"));

        var ns = NamespaceDeclaration(nsName)
            .WithMembers(SingletonList<MemberDeclarationSyntax>(attrClass));

        var cu = CompilationUnit()
            .WithUsings(List([
                UsingDirective(IdentifierName("System"))
            ]))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(ns))
            .NormalizeWhitespace();

        return cu;
    }

    private static bool HasGenerateBindAttribute(INamedTypeSymbol symbol)
    {
        foreach (var name in symbol.GetAttributes().Select(a => a.AttributeClass?.Name))
        {
            if (name is "GenerateBindAttribute" or "GenerateBind") return true;
        }

        return false;
    }

    private static SourceText? GenerateForClass(INamedTypeSymbol classSymbol, List<Diagnostic> diagnostics)
    {
        var props = classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public).ToArray();
        if (props.Length == 0) return null;

        // Validation: binding attributes, no initializers, required modifier, get; init;
        foreach (var p in props)
        {
            var loc = p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;
            var propSyntax = p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;

            // 1) Binding attribute check (FromRoute/FromQuery/FromBody)
            var hasFromRoute = HasAttr(p, "FromRoute");
            var hasFromQuery = HasAttr(p, "FromQuery");
            var hasFromBody = HasAttr(p, "FromBody");
            var cnt = (hasFromRoute ? 1 : 0) + (hasFromQuery ? 1 : 0) + (hasFromBody ? 1 : 0);
            if (cnt != 1)
            {
                diagnostics.Add(Diagnostic.Create(MissingBindingAttribute, loc, p.Name));
            }

            // 2) Prohibit initializer on properties in the annotated class
            if (propSyntax?.Initializer != null)
            {
                diagnostics.Add(Diagnostic.Create(InitializerNotAllowed, loc, p.Name));
            }

            // 3) Require 'required' modifier on public properties.
            var hasRequiredModifier = propSyntax?.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)) ?? false;
            if (!hasRequiredModifier)
            {
                diagnostics.Add(Diagnostic.Create(PropertyMustBeRequired, loc, p.Name));
            }

            // 4) Require accessors 'get; init;'
            var hasGet = false;
            var hasInit = false;
            if (propSyntax?.AccessorList != null)
            {
                foreach (var acc in propSyntax.AccessorList.Accessors)
                {
                    if (acc.Kind() == SyntaxKind.GetAccessorDeclaration) hasGet = true;
                    if (acc.Kind() == SyntaxKind.InitAccessorDeclaration) hasInit = true;
                }
            }
            else
            {
                // expression-bodied or other unusual property forms are not acceptable here
                hasGet = false;
                hasInit = false;
            }

            if (!hasGet || !hasInit)
            {
                diagnostics.Add(Diagnostic.Create(PropertyMustHaveGetInit, loc, p.Name));
            }
        }

        // New check: Defaults must not contain members that don't correspond to public properties
        try
        {
            var defaultsType = classSymbol.GetTypeMembers("Defaults").FirstOrDefault();
            if (defaultsType != null)
            {
                var propNames = new HashSet<string>(props.Select(p => p.Name));
                foreach (var member in defaultsType.GetMembers().Where(member => !member.IsImplicitlyDeclared))
                {
                    // consider only fields and properties (ignore methods, nested types, events, etc.)
                    if (member is not (IFieldSymbol or IPropertySymbol)) continue;
                    if (propNames.Contains(member.Name)) continue;
                    // get location if possible (member declaration), otherwise fallback to Defaults type location
                    var loc = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation()
                              ?? defaultsType.Locations.FirstOrDefault() ?? Location.None;
                    diagnostics.Add(Diagnostic.Create(DefaultsMemberNotFound, loc, member.Name));
                }
            }
        }
        catch
        {
            // swallow errors in diagnostic-checking logic to avoid generator crash; generation will continue/error elsewhere
        }

        if (diagnostics.Count > 0) return null;

        var usings = List([
            UsingDirective(IdentifierName("System")),
            UsingDirective(IdentifierName("System.Threading.Tasks")),
            UsingDirective(IdentifierName("Microsoft.AspNetCore.Http")),
            UsingDirective(IdentifierName("System.Reflection")),
            UsingDirective(IdentifierName("Microsoft.Extensions.Primitives"))
        ]);

        // class declared as "partial class" (no access modifier)
        var classDeclaration = ClassDeclaration(classSymbol.Name)
            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)));

        var method = BuildBindAsyncMethod(classSymbol, props);
        classDeclaration = classDeclaration.AddMembers(method);

        MemberDeclarationSyntax top = classDeclaration;
        var ns = classSymbol.ContainingNamespace;
        if (ns is { IsGlobalNamespace: false })
        {
            var nsName = BuildQualifiedNameFromNamespace(ns);
            // file-scoped namespace
            top = FileScopedNamespaceDeclaration(nsName)
                .WithMembers(SingletonList(top));
        }

        var cu = CompilationUnit()
            .WithUsings(usings)
            .WithMembers(SingletonList(top))
            .NormalizeWhitespace();

        return SourceText.From(cu.ToFullString(), Encoding.UTF8);
    }

    private static MethodDeclarationSyntax BuildBindAsyncMethod(INamedTypeSymbol classSymbol, IPropertySymbol[] props)
    {
        var classType = BuildQualifiedType(classSymbol);
        var nullableClassType = NullableType(classType);

        var valueTaskGeneric = GenericName(Identifier("ValueTask"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SingletonSeparatedList<TypeSyntax>(nullableClassType)));

        var method = MethodDeclaration(valueTaskGeneric, "BindAsync")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(SeparatedList([
                Parameter(Identifier("context"))
                    .WithType(IdentifierName("HttpContext")),
                Parameter(Identifier("parameter"))
                    .WithType(IdentifierName("ParameterInfo"))
            ])));

        var statements = new List<StatementSyntax>();

        foreach (var p in props)
        {
            var propName = p.Name;
            var localName = ToLowerCamel(propName);
            var rawName = localName + "Raw";
            var resultName = localName + "Result";

            var hasFromRoute = HasAttr(p, "FromRoute");
            var hasFromQuery = HasAttr(p, "FromQuery");
            var hasFromBody = HasAttr(p, "FromBody");

            if (hasFromRoute)
            {
                // TryGetValue: context.Request.RouteValues.TryGetValue("key", out var raw)
                var tryGet = MakeMemberInvocation(["context", "Request", "RouteValues"], "TryGetValue",
                    ArgumentList(SeparatedList([
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal(ToLowerCamel(propName)))),
                        Argument(DeclarationExpression(
                                IdentifierName("var"),
                                SingleVariableDesignation(Identifier(rawName))))
                            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                    ])));

                // if (!TryGetValue) throw new ValidationException("Route value \"x\" is required");
                var missingMsg = LiteralExpression(SyntaxKind.StringLiteralExpression,
                    Literal($"Route value \"{ToLowerCamel(propName)}\" is required"));

                var throwRequired = ThrowStatement(
                    ObjectCreationExpression(
                            ParseTypeName(
                                "global::System.ComponentModel.DataAnnotations.ValidationException"))
                        .WithArgumentList(ArgumentList(
                            SingletonSeparatedList(Argument(missingMsg)))));

                var ifMissing = IfStatement(
                    PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryGet),
                    Block(throwRequired)
                );

                statements.Add(ifMissing);

                // Теперь: if (raw is not string <localString> || !TargetType.TryParse(<localString>, out var <parsedVar>))
                //           throw new ValidationException("Invalid input for property \"x\"");
                var localStringName = localName + "String"; // e.g. categoryIdString
                // pattern: raw is not string <localString>
                var isNotStringPattern = IsPatternExpression(
                    IdentifierName(rawName),
                    UnaryPattern(
                        DeclarationPattern(
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            SingleVariableDesignation(Identifier(localStringName))
                        )
                    )
                );

                // out var <parsedVar>  -> хотим "out var categoryId" (имя совпадает с localName)
                var outDeclExpr = DeclarationExpression(
                    IdentifierName("var"),
                    SingleVariableDesignation(Identifier(localName))
                );
                var outArg = Argument(outDeclExpr)
                    .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

                // Build TryParse invocation depending on target type
                ExpressionSyntax tryParseInvocation;
                var effectiveType = p.Type;
                if (effectiveType.TypeKind == TypeKind.Enum)
                {
                    // global::System.Enum.TryParse<EnumType>(localString, out var parsed)
                    var enumTypeArg = BuildQualifiedType(effectiveType);
                    var genericTryParse = GenericName(Identifier("TryParse"))
                        .WithTypeArgumentList(TypeArgumentList(
                            SingletonSeparatedList(enumTypeArg)));
                    var enumAccess = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        ParseName("global::System.Enum"), genericTryParse);

                    var argListEnum = ArgumentList(SeparatedList([
                        Argument(IdentifierName(localStringName)),
                        outArg
                    ]));
                    tryParseInvocation = InvocationExpression(enumAccess, argListEnum);
                }
                else
                {
                    // TargetType.TryParse(localString, [maybe middle null], out var parsed)
                    var tryParseArgsList = new List<ArgumentSyntax>
                    {
                        Argument(IdentifierName(localStringName))
                    };

                    if (HasTryParseMiddleParameter(effectiveType))
                    {
                        tryParseArgsList.Add(Argument(
                            LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    }

                    tryParseArgsList.Add(outArg);

                    tryParseInvocation =
                        MakeStaticInvocationForType(effectiveType, "TryParse", tryParseArgsList.ToArray());
                }

                // Combine: (raw is not string <localString>) || !TryParse(...)
                var notTryParse =
                    PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryParseInvocation);
                var orExpr =
                    BinaryExpression(SyntaxKind.LogicalOrExpression, isNotStringPattern, notTryParse);

                var invalidMsg = LiteralExpression(SyntaxKind.StringLiteralExpression,
                    Literal($"Invalid input for route value \"{ToLowerCamel(propName)}\""));

                var throwInvalid = ThrowStatement(
                    ObjectCreationExpression(
                            ParseTypeName(
                                "global::System.ComponentModel.DataAnnotations.ValidationException"))
                        .WithArgumentList(ArgumentList(
                            SingletonSeparatedList(Argument(invalidMsg)))));

                var ifInvalid = IfStatement(orExpr, Block(throwInvalid));
                statements.Add(ifInvalid);
            }
            else if (hasFromQuery)
            {
                // declare local: Type localName;
                var localDecl = LocalDeclarationStatement(
                    VariableDeclaration(BuildQualifiedType(p.Type))
                        .WithVariables(SingletonSeparatedList(
                            VariableDeclarator(Identifier(localName))))
                );
                statements.Add(localDecl);

                var queryTry = MakeMemberInvocation(["context", "Request", "Query"], "TryGetValue",
                    ArgumentList(SeparatedList([
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal(ToLowerCamel(propName)))),
                        Argument(DeclarationExpression(
                                IdentifierName("var"),
                                SingleVariableDesignation(Identifier(rawName))))
                            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                    ])));

                StatementSyntax bodyStmt;

                if (IsNullableLike(p.Type))
                {
                    // (unchanged) - nullable-like handling: try-parse -> assign or assign null
                    var innerType = GetInnerTypeSymbol(p.Type);
                    var isEnum = innerType.TypeKind == TypeKind.Enum;

                    var rawArg = Argument(IdentifierName(rawName));
                    var outDeclExpr = DeclarationExpression(IdentifierName("var"),
                        SingleVariableDesignation(Identifier(resultName)));
                    var outArg = Argument(outDeclExpr)
                        .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

                    ExpressionSyntax tryParseInvocation;
                    if (isEnum)
                    {
                        var enumTypeArg = BuildQualifiedType(innerType);
                        var genericTryParse = GenericName(Identifier("TryParse"))
                            .WithTypeArgumentList(TypeArgumentList(
                                SingletonSeparatedList(enumTypeArg)));
                        var enumAccess = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            ParseName("global::System.Enum"), genericTryParse);
                        var argListEnum = ArgumentList(SeparatedList([rawArg, outArg]));
                        tryParseInvocation = InvocationExpression(enumAccess, argListEnum);
                    }
                    else
                    {
                        var tryParseArgsList = new List<ArgumentSyntax> { rawArg };

                        if (HasTryParseMiddleParameter(innerType))
                            tryParseArgsList.Add(
                                Argument(
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)));

                        tryParseArgsList.Add(outArg);

                        tryParseInvocation =
                            MakeStaticInvocationForType(innerType, "TryParse", tryParseArgsList.ToArray());
                    }

                    var thenBlock = Block(ExpressionStatement(
                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(localName), IdentifierName(resultName))
                    ));
                    var elseBlock = Block(ExpressionStatement(
                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(localName),
                            LiteralExpression(SyntaxKind.NullLiteralExpression))
                    ));

                    var ifParse = IfStatement(tryParseInvocation, thenBlock,
                        ElseClause(elseBlock));
                    bodyStmt = ifParse;
                }
                else
                {
                    // NON-NULLABLE case (changed): if missing -> throw KeyNotFoundException; if parse fails -> throw ArgumentException
                    var effectiveType = p.Type;
                    var isEnum = effectiveType.TypeKind == TypeKind.Enum;

                    // raw argument (pass raw value directly)
                    var rawArg = Argument(IdentifierName(rawName));

                    // out localName (reuse declared local variable)
                    var outArg = Argument(IdentifierName(localName))
                        .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

                    ExpressionSyntax tryParse;
                    if (isEnum)
                    {
                        var enumTypeArg = BuildQualifiedType(effectiveType);
                        var genericTryParse = GenericName(Identifier("TryParse"))
                            .WithTypeArgumentList(TypeArgumentList(
                                SingletonSeparatedList(enumTypeArg)));
                        var enumAccess = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            ParseName("global::System.Enum"), genericTryParse);
                        var argListEnum = ArgumentList(SeparatedList([rawArg, outArg]));
                        tryParse = InvocationExpression(enumAccess, argListEnum);
                    }
                    else
                    {
                        var tryParseArgsList = new List<ArgumentSyntax> { rawArg };

                        if (HasTryParseMiddleParameter(effectiveType))
                            tryParseArgsList.Add(
                                Argument(
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)));

                        tryParseArgsList.Add(outArg);

                        tryParse = MakeStaticInvocationForType(effectiveType, "TryParse", tryParseArgsList.ToArray());
                    }

                    // If parse fails -> throw ArgumentException($"{raw} is not a valid <param> type");

                    var invalidMsg = LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal($"Invalid input for query value \"{ToLowerCamel(propName)}\""));

                    var throwInvalid = ThrowStatement(
                        ObjectCreationExpression(
                                ParseTypeName(
                                    "global::System.ComponentModel.DataAnnotations.ValidationException"))
                            .WithArgumentList(ArgumentList(
                                SingletonSeparatedList(Argument(invalidMsg)))));


                    var ifNotParse = IfStatement(
                        PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryParse),
                        Block(throwInvalid));

                    bodyStmt = ifNotParse;
                }

                // else clause: assign Defaults.<PropName> (only), otherwise nullable->null OR for NON-NULLABLE without Defaults -> throw KeyNotFoundException
                var defaultsExpr = GetDefaultsMemberExpression(classSymbol, p.Name);
                StatementSyntax elseAssign;
                if (defaultsExpr != null)
                {
                    elseAssign = ExpressionStatement(
                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(localName),
                            defaultsExpr));
                }
                else
                {
                    if (IsNullableLike(p.Type))
                    {
                        elseAssign = ExpressionStatement(
                            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(localName),
                                LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    }
                    else
                    {
                        // NON-NULLABLE and no Defaults -> throw KeyNotFoundException("Required parameter \"x\" not found")
                        var missingMsg = LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal($"Required parameter \"{ToLowerCamel(propName)}\" not found"));
                        elseAssign = ThrowStatement(
                            ObjectCreationExpression(
                                    ParseTypeName(
                                        "global::System.Collections.Generic.KeyNotFoundException"))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                    Argument(missingMsg)))));
                    }
                }

                var ifFull = IfStatement(queryTry, Block(bodyStmt),
                    ElseClause(Block(elseAssign)));
                statements.Add(ifFull);
            }
            else if (hasFromBody)
            {
                statements.Add(
                    ParseStatement("// FromBody binding not implemented by generator\r\n"));
                statements.Add(
                    ReturnStatement(
                        LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
            }
        }

        var initializerExpressions = props.Select(ExpressionSyntax (p) =>
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(p.Name),
                IdentifierName(ToLowerCamel(p.Name)))).ToArray();

        var objInit = InitializerExpression(SyntaxKind.ObjectInitializerExpression,
            SeparatedList(initializerExpressions));
        var newObj = ObjectCreationExpression(BuildQualifiedType(classSymbol))
            .WithInitializer(objInit);

        var genericFromResult = GenericName("FromResult")
            .WithTypeArgumentList(TypeArgumentList(
                SingletonSeparatedList<TypeSyntax>(
                    NullableType(BuildQualifiedType(classSymbol)))));

        var valueTaskFromResult = InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("ValueTask"), genericFromResult),
            ArgumentList(SingletonSeparatedList(Argument(newObj))));

        statements.Add(ReturnStatement(valueTaskFromResult));

        var body = Block(statements);
        method = method.WithBody(body);

        return method;
    }


    private static bool HasAttr(IPropertySymbol p, string attrName)
    {
        return p.GetAttributes().Select(a => a.AttributeClass?.Name)
            .Any(n => n == attrName || n == attrName + "Attribute");
    }

    private static string ToLowerCamel(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static NameSyntax BuildQualifiedNameFromNamespace(INamespaceSymbol ns)
    {
        var full = ns.ToDisplayString();
        return ParseName(full);
    }

    private static TypeSyntax BuildQualifiedType(ITypeSymbol type)
    {
        var text = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return ParseTypeName(text);
    }

    private static ExpressionSyntax MakeMemberInvocation(string[] leftParts, string member, ArgumentListSyntax args)
    {
        ExpressionSyntax expr = IdentifierName(leftParts[0]);
        for (var i = 1; i < leftParts.Length; i++)
        {
            expr = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr,
                IdentifierName(leftParts[i]));
        }

        expr = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr,
            IdentifierName(member));
        return InvocationExpression(expr, args);
    }

    private static ExpressionSyntax MakeStaticInvocationForType(ITypeSymbol type, string methodName,
        params ArgumentSyntax[] args)
    {
        var typeSyntax = BuildQualifiedType(type);
        var member = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeSyntax,
            IdentifierName(methodName));
        var argList = ArgumentList(SeparatedList(args));
        return InvocationExpression(member, argList);
    }

    private static bool HasTryParseMiddleParameter(ITypeSymbol type)
    {
        try
        {
            if ((from m in type.GetMembers("TryParse").OfType<IMethodSymbol>()
                    where m.IsStatic
                    where m.Parameters.Length >= 3
                    select m.Parameters.Last()).Any(last => last.RefKind == RefKind.Out))
            {
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static bool IsNullableLike(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated) return true;
        if (type is INamedTypeSymbol named &&
            named.ConstructedFrom.ToDisplayString() == "System.Nullable<T>") return true;
        return type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.NotAnnotated;
    }

    private static ITypeSymbol GetInnerTypeSymbol(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && named.ConstructedFrom.ToDisplayString() == "System.Nullable<T>" &&
            named.TypeArguments.Length == 1)
            return named.TypeArguments[0];
        return type;
    }

    private static ExpressionSyntax? GetDefaultsMemberExpression(INamedTypeSymbol classSymbol, string propName)
    {
        try
        {
            var defaultsType = classSymbol.GetTypeMembers("Defaults").FirstOrDefault();
            var member = defaultsType?.GetMembers(propName).FirstOrDefault();
            if (member is null) return null;
            var classFq = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var exprText = $"{classFq}.Defaults.{propName}";
            return ParseExpression(exprText);
        }
        catch
        {
            return null;
        }
    }
}