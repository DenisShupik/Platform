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

    private static readonly DiagnosticDescriptor MultipleBindingAttribute = new(
        id: "GB0006",
        title: "Property must not have multiple binding attributes",
        messageFormat:
        "Property '{0}' must have exactly one binding attribute among [FromRoute], [FromQuery], [FromBody].",
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
                var generated = GenerateForClass(symbol, compilation, diagnostics);

                foreach (var d in diagnostics) spc.ReportDiagnostic(d);

                if (generated == null) continue;
                var hint = $"{symbol.Name}.GenerateBind.g.cs";
                spc.AddSource(hint, generated);
            }
        });
    }

    // --- Attribute helper ---
    private static CompilationUnitSyntax CreateGenerateBindAttribute()
    {
        var attrClass = ClassDeclaration("GenerateBindAttribute")
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(ParseTypeName("System.Attribute")))))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));

        var nsName = QualifiedName(QualifiedName(IdentifierName("SharedKernel"), IdentifierName("Presentation")),
            IdentifierName("Generator"));
        var ns = NamespaceDeclaration(nsName).WithMembers(SingletonList<MemberDeclarationSyntax>(attrClass));

        var cu = CompilationUnit()
            .WithUsings(List([UsingDirective(IdentifierName("System"))]))
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

    // --- Generation ---
    private static SourceText? GenerateForClass(INamedTypeSymbol classSymbol, Compilation compilation,
        List<Diagnostic> diagnostics)
    {
        var props = classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public).ToArray();
        if (props.Length == 0) return null;

        // 1) Validate and build a small descriptor for each property (binding + syntax node)
        var propInfos = new List<PropertyInfo>();
        foreach (var p in props)
        {
            var loc = p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;
            var propSyntax = p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;

            var binding = GetSingleBinding(p, compilation, diagnostics, loc);

            if (propSyntax?.Initializer != null)
                diagnostics.Add(Diagnostic.Create(InitializerNotAllowed, loc, p.Name));

            var hasRequiredModifier = propSyntax?.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)) ?? false;
            if (!hasRequiredModifier)
                diagnostics.Add(Diagnostic.Create(PropertyMustBeRequired, loc, p.Name));

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
                hasGet = false;
                hasInit = false;
            }

            if (!hasGet || !hasInit)
                diagnostics.Add(Diagnostic.Create(PropertyMustHaveGetInit, loc, p.Name));

            propInfos.Add(new PropertyInfo(p, binding));
        }

        // Defaults check
        try
        {
            var defaultsType = classSymbol.GetTypeMembers("Defaults").FirstOrDefault();
            if (defaultsType != null)
            {
                var propNames = new HashSet<string>(props.Select(p => p.Name));
                foreach (var member in defaultsType.GetMembers().Where(m => !m.IsImplicitlyDeclared))
                {
                    if (member is not (IFieldSymbol or IPropertySymbol)) continue;
                    if (propNames.Contains(member.Name)) continue;
                    var loc = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation()
                              ?? defaultsType.Locations.FirstOrDefault() ?? Location.None;
                    diagnostics.Add(Diagnostic.Create(DefaultsMemberNotFound, loc, member.Name));
                }
            }
        }
        catch
        {
            // ignored
        }

        if (diagnostics.Count > 0) return null;

        // --- Build method ---
        var usings = List([
            UsingDirective(IdentifierName("System")),
            UsingDirective(IdentifierName("System.Threading.Tasks")),
            UsingDirective(IdentifierName("Microsoft.AspNetCore.Http")),
            UsingDirective(IdentifierName("System.Reflection")),
            UsingDirective(IdentifierName("Microsoft.Extensions.Primitives"))
        ]);

        var classDeclaration = ClassDeclaration(classSymbol.Name)
            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)));
        var method = BuildBindAsyncMethod(classSymbol, propInfos.ToArray());
        classDeclaration = classDeclaration.AddMembers(method);

        MemberDeclarationSyntax top = classDeclaration;
        var ns = classSymbol.ContainingNamespace;
        if (ns is { IsGlobalNamespace: false })
        {
            var nsName = BuildQualifiedNameFromNamespace(ns);
            top = FileScopedNamespaceDeclaration(nsName).WithMembers(SingletonList(top));
        }

        var cu = CompilationUnit().WithUsings(usings).WithMembers(SingletonList(top)).NormalizeWhitespace();
        return SourceText.From(cu.ToFullString(), Encoding.UTF8);
    }

    // BindingKind + PropertyInfo
    private enum BindingKind
    {
        None,
        FromRoute,
        FromQuery,
        FromBody
    }

    private sealed class PropertyInfo(IPropertySymbol symbol, BindingKind binding)
    {
        public IPropertySymbol Symbol { get; } = symbol;
        public BindingKind Binding { get; } = binding;
    }

    private static MethodDeclarationSyntax BuildBindAsyncMethod(INamedTypeSymbol classSymbol, PropertyInfo[] props)
    {
        var classType = BuildQualifiedType(classSymbol);
        var nullableClassType = NullableType(classType);

        var valueTaskGeneric = GenericName(Identifier("ValueTask"))
            .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(nullableClassType)));

        var method = MethodDeclaration(valueTaskGeneric, "BindAsync")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(SeparatedList([
                Parameter(Identifier("context")).WithType(IdentifierName("HttpContext")),
                Parameter(Identifier("parameter")).WithType(IdentifierName("ParameterInfo"))
            ])));

        var statements = new List<StatementSyntax>();

        // helper local functions (keeps method body generation compact)
        static StatementSyntax ThrowValidation(string msg) => ThrowStatement(
            ObjectCreationExpression(ParseTypeName("global::System.ComponentModel.DataAnnotations.ValidationException"))
                .WithArgumentList(ArgumentList(
                    SingletonSeparatedList(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(msg)))))));

        foreach (var info in props)
        {
            var p = info.Symbol;
            var propName = p.Name;
            var localName = ToLowerCamel(propName);
            var rawName = localName + "Raw";

            switch (info.Binding)
            {
                case BindingKind.FromRoute:
                {
                    // Route: ensure existence then parse from string
                    var tryGet = MakeMemberInvocation(["context", "Request", "RouteValues"], "TryGetValue",
                        ArgumentList(SeparatedList([
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal(ToLowerCamel(propName)))),
                            Argument(DeclarationExpression(IdentifierName("var"),
                                    SingleVariableDesignation(Identifier(rawName))))
                                .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                        ])));

                    statements.Add(IfStatement(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryGet),
                        Block(ThrowValidation($"Route value \"{ToLowerCamel(propName)}\" is required"))));

                    // parse and validate string
                    statements.Add(BuildRouteParseAndValidation(p, rawName, localName, propName));
                    break;
                }
                case BindingKind.FromQuery:
                {
                    // Query: declare local, try get, then parse or fallback to Defaults/null/throw
                    statements.Add(LocalDeclarationStatement(VariableDeclaration(BuildQualifiedType(p.Type))
                        .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(localName))))));

                    var queryTry = MakeMemberInvocation(["context", "Request", "Query"], "TryGetValue",
                        ArgumentList(SeparatedList([
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal(ToLowerCamel(propName)))),
                            Argument(DeclarationExpression(IdentifierName("var"),
                                    SingleVariableDesignation(Identifier(rawName))))
                                .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                        ])));

                    var whenFound = BuildQueryFoundBody(p, rawName, localName).ToArray();
                    var whenMissing = BuildQueryMissingBody(classSymbol, p, localName, propName).ToArray();

                    statements.Add(IfStatement(queryTry, Block(whenFound), ElseClause(Block(whenMissing))));
                    break;
                }
                case BindingKind.FromBody:
                    statements.Add(ParseStatement("// FromBody binding not implemented by generator\r\n"));
                    statements.Add(ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
                    break;
                default:
                    statements.Add(ParseStatement($"// Unknown binding for property {p.Name}\r\n"));
                    break;
            }
        }

        // object initializer using locals
        var initializerExpressions = props.Select(pi =>
            (ExpressionSyntax)AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(pi.Symbol.Name), IdentifierName(ToLowerCamel(pi.Symbol.Name)))).ToArray();
        var objInit =
            InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(initializerExpressions));
        var newObj = ObjectCreationExpression(BuildQualifiedType(classSymbol)).WithInitializer(objInit);

        var genericFromResult = GenericName("FromResult").WithTypeArgumentList(
            TypeArgumentList(SingletonSeparatedList<TypeSyntax>(NullableType(BuildQualifiedType(classSymbol)))));
        var valueTaskFromResult =
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("ValueTask"),
                    genericFromResult), ArgumentList(SingletonSeparatedList(Argument(newObj))));

        statements.Add(ReturnStatement(valueTaskFromResult));

        method = method.WithBody(Block(statements));
        return method;

        // helper local functions (keeps method body generation compact)
        static StatementSyntax BuildRouteParseAndValidation(IPropertySymbol p, string rawName, string localName,
            string propName)
        {
            // raw is not string <s> || !TryParse(<s>, out var local)
            var localString = localName + "String";
            var isNotStringPattern = IsPatternExpression(IdentifierName(rawName),
                UnaryPattern(DeclarationPattern(PredefinedType(Token(SyntaxKind.StringKeyword)),
                    SingleVariableDesignation(Identifier(localString)))));

            var outDeclExpr =
                DeclarationExpression(IdentifierName("var"), SingleVariableDesignation(Identifier(localName)));
            var outArg = Argument(outDeclExpr).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

            ExpressionSyntax tryParseInvocation;
            if (p.Type.TypeKind == TypeKind.Enum)
            {
                var enumType = BuildQualifiedType(p.Type);
                var enumTry = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    ParseName("global::System.Enum"),
                    GenericName(Identifier("TryParse"))
                        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(enumType))));
                tryParseInvocation = InvocationExpression(enumTry, ArgumentList(SeparatedList([
                    Argument(IdentifierName(localString)), outArg
                ])));
            }
            else
            {
                var args = new List<ArgumentSyntax> { Argument(IdentifierName(localString)) };
                if (HasTryParseMiddleParameter(p.Type))
                    args.Add(Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)));
                args.Add(outArg);
                tryParseInvocation = MakeStaticInvocationForType(p.Type, "TryParse", args.ToArray());
            }

            var notTry = PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryParseInvocation);
            var orExpr = BinaryExpression(SyntaxKind.LogicalOrExpression, isNotStringPattern, notTry);

            var throwInvalid = ThrowValidation($"Invalid input for route value \"{ToLowerCamel(propName)}\"");
            return IfStatement(orExpr, Block(throwInvalid));
        }

        static IEnumerable<StatementSyntax> BuildQueryFoundBody(IPropertySymbol p, string rawName, string localName)
        {
            // returns statements for the "found" branch (either assign parsed value or throw for non-nullable)
            var stmts = new List<StatementSyntax>();

            if (IsNullableLike(p.Type))
            {
                var inner = GetInnerTypeSymbol(p.Type);
                var outDecl = DeclarationExpression(IdentifierName("var"),
                    SingleVariableDesignation(Identifier(localName + "Result")));
                var outArg = Argument(outDecl).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

                ExpressionSyntax tryParse;
                if (inner.TypeKind == TypeKind.Enum)
                {
                    var enumType = BuildQualifiedType(inner);
                    var enumTry = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        ParseName("global::System.Enum"),
                        GenericName(Identifier("TryParse"))
                            .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(enumType))));
                    tryParse = InvocationExpression(enumTry, ArgumentList(SeparatedList([
                        Argument(IdentifierName(rawName)), outArg
                    ])));
                }
                else
                {
                    var args = new List<ArgumentSyntax> { Argument(IdentifierName(rawName)) };
                    if (HasTryParseMiddleParameter(inner))
                        args.Add(Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    args.Add(outArg);
                    tryParse = MakeStaticInvocationForType(inner, "TryParse", args.ToArray());
                }

                // if (TryParse(...)) local = result; else local = null;
                var thenBlock = Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(localName), IdentifierName(localName + "Result"))));
                var elseBlock = Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(localName), LiteralExpression(SyntaxKind.NullLiteralExpression))));
                stmts.Add(IfStatement(tryParse, thenBlock, ElseClause(elseBlock)));
            }
            else
            {
                var outArg = Argument(IdentifierName(localName)).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));
                ExpressionSyntax tryParse;
                if (p.Type.TypeKind == TypeKind.Enum)
                {
                    var enumType = BuildQualifiedType(p.Type);
                    var enumTry = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        ParseName("global::System.Enum"),
                        GenericName(Identifier("TryParse"))
                            .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(enumType))));
                    tryParse = InvocationExpression(enumTry, ArgumentList(SeparatedList([
                        Argument(IdentifierName(rawName)), outArg
                    ])));
                }
                else
                {
                    var args = new List<ArgumentSyntax> { Argument(IdentifierName(rawName)) };
                    if (HasTryParseMiddleParameter(p.Type))
                        args.Add(Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    args.Add(outArg);
                    tryParse = MakeStaticInvocationForType(p.Type, "TryParse", args.ToArray());
                }

                var throwInvalid = ThrowValidation($"Invalid input for query value \"{ToLowerCamel(p.Name)}\"");
                stmts.Add(IfStatement(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryParse),
                    Block(throwInvalid)));
            }

            return stmts;
        }

        static IEnumerable<StatementSyntax> BuildQueryMissingBody(INamedTypeSymbol classSymbol, IPropertySymbol p,
            string localName, string propName)
        {
            var stmts = new List<StatementSyntax>();
            var defaultsExpr = GetDefaultsMemberExpression(classSymbol, p.Name);
            if (defaultsExpr != null)
            {
                stmts.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(localName), defaultsExpr)));
            }
            else if (IsNullableLike(p.Type))
            {
                stmts.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(localName), LiteralExpression(SyntaxKind.NullLiteralExpression))));
            }
            else
            {
                var missingMsg = LiteralExpression(SyntaxKind.StringLiteralExpression,
                    Literal($"Required parameter \"{ToLowerCamel(propName)}\" not found"));
                stmts.Add(ThrowStatement(
                    ObjectCreationExpression(ParseTypeName("global::System.Collections.Generic.KeyNotFoundException"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(missingMsg))))));
            }

            return stmts;
        }
    }

    // --- Utilities (kept small and shared) ---
    private static BindingKind GetSingleBinding(IPropertySymbol p, Compilation compilation,
        List<Diagnostic> diagnostics, Location loc)
    {
        // Try to resolve attribute symbols from compilation first (robust)
        var fromRouteSym = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromRouteAttribute");
        var fromQuerySym = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
        var fromBodySym = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromBodyAttribute");

        var found = new List<BindingKind>();

        foreach (var cls in p.GetAttributes().Select(a => a.AttributeClass).OfType<INamedTypeSymbol>())
        {
            if (fromRouteSym != null && SymbolEqualityComparer.Default.Equals(cls, fromRouteSym))
            {
                found.Add(BindingKind.FromRoute);
                continue;
            }

            if (fromQuerySym != null && SymbolEqualityComparer.Default.Equals(cls, fromQuerySym))
            {
                found.Add(BindingKind.FromQuery);
                continue;
            }

            if (fromBodySym != null && SymbolEqualityComparer.Default.Equals(cls, fromBodySym))
            {
                found.Add(BindingKind.FromBody);
                continue;
            }

            // fallback: name-based detection to preserve compatibility
            var n = cls.Name;
            if (n.EndsWith("Attribute")) n = n.Substring(0, n.Length - "Attribute".Length);
            switch (n)
            {
                case "FromRoute":
                    found.Add(BindingKind.FromRoute);
                    break;
                case "FromQuery":
                    found.Add(BindingKind.FromQuery);
                    break;
                case "FromBody":
                    found.Add(BindingKind.FromBody);
                    break;
            }
        }

        switch (found.Count)
        {
            case 0:
                diagnostics.Add(Diagnostic.Create(MissingBindingAttribute, loc, p.Name));
                return BindingKind.None;
            case > 1:
                diagnostics.Add(Diagnostic.Create(MultipleBindingAttribute, loc, p.Name));
                return BindingKind.None;
            default:
                return found[0];
        }
    }

    private static string ToLowerCamel(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static NameSyntax BuildQualifiedNameFromNamespace(INamespaceSymbol ns) => ParseName(ns.ToDisplayString());

    private static TypeSyntax BuildQualifiedType(ITypeSymbol type) =>
        ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

    private static ExpressionSyntax MakeMemberInvocation(string[] leftParts, string member, ArgumentListSyntax args)
    {
        ExpressionSyntax expr = IdentifierName(leftParts[0]);
        for (var i = 1; i < leftParts.Length; i++)
            expr = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, IdentifierName(leftParts[i]));
        expr = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, IdentifierName(member));
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
            return (from m in type.GetMembers("TryParse").OfType<IMethodSymbol>()
                where m.IsStatic
                where m.Parameters.Length >= 3
                select m.Parameters.Last()).Any(last => last.RefKind == RefKind.Out);
        }
        catch
        {
            return false;
        }
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
            named.TypeArguments.Length == 1) return named.TypeArguments[0];
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