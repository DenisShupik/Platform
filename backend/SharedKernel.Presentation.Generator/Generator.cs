using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        messageFormat: "Property '{0}' must not have an initializer in a type annotated with [GenerateBind]. Move default values to the nested Defaults class.",
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
        messageFormat: "Defaults contains member '{0}' but no matching public property '{0}' exists in the enclosing [GenerateBind] type.",
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

                if (generated != null)
                {
                    var hint = $"{symbol.Name}.GenerateBind.g.cs";
                    spc.AddSource(hint, generated);
                }
            }
        });
    }

    private static CompilationUnitSyntax CreateGenerateBindAttribute()
    {
        var attrClass = SyntaxFactory.ClassDeclaration("GenerateBindAttribute")
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.InternalKeyword),
                SyntaxFactory.Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("System.Attribute")))))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

        // Построение SharedKernel.Presentation.Generator
        NameSyntax nsName =
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("SharedKernel"),
                    SyntaxFactory.IdentifierName("Presentation")),
                SyntaxFactory.IdentifierName("Generator"));

        var ns = SyntaxFactory.NamespaceDeclaration(nsName)
            .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(attrClass));

        var cu = SyntaxFactory.CompilationUnit()
            .WithUsings(SyntaxFactory.List(new[] {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System"))
            }))
            .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(ns))
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
                foreach (var member in defaultsType.GetMembers())
                {
                    if (member.IsImplicitlyDeclared) continue;

                    // consider only fields and properties (ignore methods, nested types, events, etc.)
                    if (member is IFieldSymbol || member is IPropertySymbol)
                    {
                        if (!propNames.Contains(member.Name))
                        {
                            // get location if possible (member declaration), otherwise fallback to Defaults type location
                            var loc = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation()
                                      ?? defaultsType.Locations.FirstOrDefault() ?? Location.None;
                            diagnostics.Add(Diagnostic.Create(DefaultsMemberNotFound, loc, member.Name));
                        }
                    }
                }
            }
        }
        catch
        {
            // swallow errors in diagnostic-checking logic to avoid generator crash; generation will continue/error elsewhere
        }

        if (diagnostics.Count > 0) return null;

        var usings = SyntaxFactory.List(new[] {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Threading.Tasks")),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Http")),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Reflection")),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.Extensions.Primitives"))
        });

        // class declared as "partial class" (no access modifier)
        var classDeclaration = SyntaxFactory.ClassDeclaration(classSymbol.Name)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

        var method = BuildBindAsyncMethod(classSymbol, props);
        classDeclaration = classDeclaration.AddMembers(method);

        MemberDeclarationSyntax top = classDeclaration;
        var ns = classSymbol.ContainingNamespace;
        if (ns is { IsGlobalNamespace: false })
        {
            var nsName = BuildQualifiedNameFromNamespace(ns);
            // file-scoped namespace
            top = SyntaxFactory.FileScopedNamespaceDeclaration(nsName)
                .WithMembers(SyntaxFactory.SingletonList(top));
        }

        var cu = SyntaxFactory.CompilationUnit()
            .WithUsings(usings)
            .WithMembers(SyntaxFactory.SingletonList(top))
            .NormalizeWhitespace();

        return SourceText.From(cu.ToFullString(), Encoding.UTF8);
    }

    private static MethodDeclarationSyntax BuildBindAsyncMethod(INamedTypeSymbol classSymbol, IPropertySymbol[] props)
    {
        var classType = BuildQualifiedType(classSymbol);
        var nullableClassType = SyntaxFactory.NullableType(classType);

        var valueTaskGeneric = SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueTask"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(nullableClassType)));

        var method = SyntaxFactory.MethodDeclaration(valueTaskGeneric, "BindAsync")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new ParameterSyntax[] {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                    .WithType(SyntaxFactory.IdentifierName("HttpContext")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parameter"))
                    .WithType(SyntaxFactory.IdentifierName("ParameterInfo"))
            })));

        var statements = new List<StatementSyntax>();

        foreach (var p in props)
        {
            var propSyntax = p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
            var propName = p.Name;
            var localName = ToLowerCamel(propName);
            var rawName = localName + "Raw";
            var resultName = localName + "Result";

            var hasFromRoute = HasAttr(p, "FromRoute");
            var hasFromQuery = HasAttr(p, "FromQuery");
            var hasFromBody = HasAttr(p, "FromBody");

            if (hasFromRoute)
            {
                var tryGet = MakeMemberInvocation(new[] { "context", "Request", "RouteValues" }, "TryGetValue",
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(ToRouteKey(propName)))),
                        SyntaxFactory.Argument(SyntaxFactory.DeclarationExpression(
                                SyntaxFactory.IdentifierName("var"),
                                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(rawName))))
                            .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                    })));

                var isNotPattern = SyntaxFactory.IsPatternExpression(
                    SyntaxFactory.IdentifierName(rawName),
                    SyntaxFactory.UnaryPattern(
                        SyntaxFactory.DeclarationPattern(
                            BuildQualifiedType(p.Type),
                            SyntaxFactory.SingleVariableDesignation(
                                SyntaxFactory.Identifier(localName)
                            )
                        )
                    )
                );
                var andExpr = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, tryGet, isNotPattern);
                var notExpr = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, andExpr);

                var returnDefault =
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
                var ifStmt = SyntaxFactory.IfStatement(notExpr, SyntaxFactory.Block(returnDefault));
                statements.Add(ifStmt);
            }
            else if (hasFromQuery)
            {
                // declare local: Type localName;
                var localDecl = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(BuildQualifiedType(p.Type))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(localName))))
                );
                statements.Add(localDecl);

                var queryTry = MakeMemberInvocation(new[] { "context", "Request", "Query" }, "TryGetValue",
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(ToRouteKey(propName)))),
                        SyntaxFactory.Argument(SyntaxFactory.DeclarationExpression(
                                SyntaxFactory.IdentifierName("var"),
                                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(rawName))))
                            .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                    })));

                StatementSyntax bodyStmt;

                if (IsNullableLike(p.Type))
                {
                    var innerType = GetInnerTypeSymbol(p.Type);

                    var rawArg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(rawName));

                    // out var result
                    var outDeclExpr = SyntaxFactory.DeclarationExpression(SyntaxFactory.IdentifierName("var"),
                        SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(resultName)));
                    var outArg = SyntaxFactory.Argument(outDeclExpr)
                        .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword));

                    var tryParseArgsList = new List<ArgumentSyntax> { rawArg };

                    if (HasTryParseMiddleParameter(innerType))
                        tryParseArgsList.Add(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));

                    tryParseArgsList.Add(outArg);

                    var tryParseInvocation =
                        MakeStaticInvocationForType(innerType, "TryParse", tryParseArgsList.ToArray());

                    var thenBlock = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(localName), SyntaxFactory.IdentifierName(resultName))
                    ));
                    var elseBlock = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(localName),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                    ));

                    var ifParse = SyntaxFactory.IfStatement(tryParseInvocation, thenBlock,
                        SyntaxFactory.ElseClause(elseBlock));
                    bodyStmt = ifParse;
                }
                else
                {
                    var rawArg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(rawName));
                    // out localName (reuse declared local variable)
                    var outArg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(localName))
                        .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword));

                    var tryParseArgsList = new List<ArgumentSyntax> { rawArg };

                    if (HasTryParseMiddleParameter(p.Type))
                        tryParseArgsList.Add(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));

                    tryParseArgsList.Add(outArg);

                    var tryParse = MakeStaticInvocationForType(p.Type, "TryParse", tryParseArgsList.ToArray());

                    var ifNotParse = SyntaxFactory.IfStatement(
                        SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, tryParse),
                        SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)))));

                    bodyStmt = ifNotParse;
                }

                // else clause: assign Defaults.<PropName> (only), otherwise nullable->null or default(Type)
                var defaultsExpr = GetDefaultsMemberExpression(classSymbol, p.Name);
                StatementSyntax elseAssign;
                if (defaultsExpr != null)
                {
                    elseAssign = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(localName),
                            defaultsExpr));
                }
                else
                {
                    if (IsNullableLike(p.Type))
                    {
                        elseAssign = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(localName),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
                    }
                    else
                    {
                        // только default(Type) — НЕ ищем Type.Default и т.п.
                        elseAssign = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(localName),
                                SyntaxFactory.DefaultExpression(BuildQualifiedType(p.Type))));
                    }
                }

                var ifFull = SyntaxFactory.IfStatement(queryTry, SyntaxFactory.Block(bodyStmt),
                    SyntaxFactory.ElseClause(SyntaxFactory.Block(elseAssign)));
                statements.Add(ifFull);
            }
            else if (hasFromBody)
            {
                statements.Add(
                    SyntaxFactory.ParseStatement("// FromBody binding not implemented by generator\r\n"));
                statements.Add(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
            }
        }

        var initializerExpressions = props.Select(p =>
            (ExpressionSyntax)SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(p.Name),
                SyntaxFactory.IdentifierName(ToLowerCamel(p.Name)))).ToArray();

        var objInit = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
            SyntaxFactory.SeparatedList(initializerExpressions));
        var newObj = SyntaxFactory.ObjectCreationExpression(BuildQualifiedType(classSymbol))
            .WithInitializer(objInit);

        var genericFromResult = SyntaxFactory.GenericName("FromResult")
            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                    SyntaxFactory.NullableType(BuildQualifiedType(classSymbol)))));

        var valueTaskFromResult = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("ValueTask"), genericFromResult),
            SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(newObj))));

        statements.Add(SyntaxFactory.ReturnStatement(valueTaskFromResult));

        var body = SyntaxFactory.Block(statements);
        method = method.WithBody(body);

        return method;
    }

    private static bool HasAttr(IPropertySymbol p, string attrName)
    {
        return p.GetAttributes().Select(a => a.AttributeClass?.Name)
            .Any(n => n == attrName || n == (attrName + "Attribute"));
    }

    private static string ToRouteKey(string propName)
    {
        if (string.IsNullOrEmpty(propName)) return propName;
        return char.ToLowerInvariant(propName[0]) + propName.Substring(1);
    }

    private static string ToLowerCamel(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static NameSyntax BuildQualifiedNameFromNamespace(INamespaceSymbol ns)
    {
        var full = ns.ToDisplayString();
        return SyntaxFactory.ParseName(full);
    }

    private static TypeSyntax BuildQualifiedType(ITypeSymbol type)
    {
        var text = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return SyntaxFactory.ParseTypeName(text);
    }

    private static ExpressionSyntax MakeMemberInvocation(string[] leftParts, string member, ArgumentListSyntax args)
    {
        ExpressionSyntax expr = SyntaxFactory.IdentifierName(leftParts[0]);
        for (var i = 1; i < leftParts.Length; i++)
        {
            expr = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr,
                SyntaxFactory.IdentifierName(leftParts[i]));
        }

        expr = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr,
            SyntaxFactory.IdentifierName(member));
        return SyntaxFactory.InvocationExpression(expr, args);
    }

    private static ExpressionSyntax MakeStaticInvocationForType(ITypeSymbol type, string methodName,
        params ArgumentSyntax[] args)
    {
        var typeSyntax = BuildQualifiedType(type);
        var member = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeSyntax,
            SyntaxFactory.IdentifierName(methodName));
        var argList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args));
        return SyntaxFactory.InvocationExpression(member, argList);
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
            // найти вложенный тип Defaults
            var defaultsType = classSymbol.GetTypeMembers("Defaults").FirstOrDefault();
            if (defaultsType is null) return null;

            // есть ли член с таким именем (поле/свойство/const)
            var member = defaultsType.GetMembers(propName).FirstOrDefault();
            if (member is null) return null;

            // сформировать fully-qualified выражение: global::...MyClass.Defaults.PropName
            var classFq = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var exprText = $"{classFq}.Defaults.{propName}";
            return SyntaxFactory.ParseExpression(exprText);
        }
        catch
        {
            return null;
        }
    }
}
