using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor PropertyTypeMismatch = new(
        id: "CRG001",
        title: "Property type mismatch",
        messageFormat: "Type in typeof({0}) does not match type in nameof({1}.{2})",
        category: nameof(Generator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (node, _) =>
                            node is ClassDeclarationSyntax cds
                            && cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                            && cds.AttributeLists.Count > 0,
                        transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                    .Where(cd => cd is not null)
                    .Collect()),
            static (spc, source) => Execute(spc, source.Left, source.Right));
    }

    private static void Execute(
        SourceProductionContext spc,
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var omitSym = compilation.GetTypeByMetadataName("Generator.Attributes.OmitAttribute");
        var asReqSym = compilation.GetTypeByMetadataName("Generator.Attributes.IncludeAsRequiredAttribute");
        if (omitSym is null || asReqSym is null) return;

        foreach (var classDecl in classes)
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (model.GetDeclaredSymbol(classDecl) is not { } target) continue;

            var entries = ImmutableArray.CreateBuilder<(INamedTypeSymbol Source, string Name, bool Required)>();

            var omitAttrs = target.GetAttributes()
                .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, omitSym))
                .ToArray();

            foreach (var attr in classDecl.AttributeLists.SelectMany(al => al.Attributes))
            {
                var attrSym = model.GetSymbolInfo(attr).Symbol?.ContainingType;
                if (attrSym is null ||
                    (!SymbolEqualityComparer.Default.Equals(attrSym, asReqSym) &&
                     !SymbolEqualityComparer.Default.Equals(attrSym, omitSym)))
                    continue;

                var args = attr.ArgumentList?.Arguments;
                if (args is null || args.Value.Count < 2) continue;

                var typeInfo = args.Value[0].Expression is TypeOfExpressionSyntax typeOfExpr
                    ? model.GetTypeInfo(typeOfExpr.Type).Type as INamedTypeSymbol
                    : null;
                if (typeInfo is null) continue;

                for (int i = 1; i < args.Value.Count; i++)
                {
                    if (args.Value[i].Expression is not InvocationExpressionSyntax inv
                        || inv.Expression is not IdentifierNameSyntax idn
                        || idn.Identifier.Text != "nameof"
                        || inv.ArgumentList.Arguments.Count == 0
                        || inv.ArgumentList.Arguments[0].Expression is not MemberAccessExpressionSyntax ma)
                        continue;

                    if (model.GetTypeInfo(ma.Expression).Type is INamedTypeSymbol leftType
                        && !SymbolEqualityComparer.Default.Equals(leftType, typeInfo))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(
                            PropertyTypeMismatch,
                            args.Value[i].GetLocation(),
                            typeInfo.ToDisplayString(),
                            leftType.ToDisplayString(),
                            ma.Name.Identifier.Text));
                    }
                }
            }

            foreach (var attr in target.GetAttributes()
                         .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, asReqSym)))
            {
                if (attr.ConstructorArguments.Length < 2
                    || attr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType)
                    continue;

                foreach (var name in attr.ConstructorArguments[1].Values
                             .Select(nameObj => nameObj.Value as string)
                             .Where(name => !string.IsNullOrWhiteSpace(name))
                        )
                {
                    if (name != null)
                    {
                        var propSym = srcType.GetMembers(name)
                            .OfType<IPropertySymbol>()
                            .FirstOrDefault();
                        if (propSym is null || !SymbolEqualityComparer.Default.Equals(propSym.ContainingType, srcType))
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                PropertyTypeMismatch,
                                classDecl.GetLocation(),
                                srcType.ToDisplayString(),
                                srcType.ToDisplayString(),
                                name));
                            continue;
                        }
                    }

                    entries.Add((srcType, name!, true));
                }
            }

            foreach (var omitAttr in omitAttrs)
            {
                if (omitAttr.ConstructorArguments.Length < 2
                    || omitAttr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType)
                    continue;

                var omitted = omitAttr.ConstructorArguments[1].Values
                    .Select(v => v.Value as string)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToImmutableHashSet();

                foreach (var prop in srcType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (!omitted.Contains(prop.Name))
                        entries.Add((srcType, prop.Name, false));
                }
            }

            if (entries.Count == 0)
                continue;

            var sourceType = entries[0].Source;
            var members = sourceType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => entries.Any(e => e.Name == p.Name))
                .Select(p =>
                {
                    var req = entries.First(e => e.Name == p.Name).Required;
                    return CreateProperty(p, req);
                })
                .ToArray<MemberDeclarationSyntax>();

            var nsName = target.ContainingNamespace?.ToDisplayString() ?? "Generated";
            var fileNs = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(nsName))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                        .WithTrailingTrivia(SyntaxFactory.LineFeed))
                .AddMembers(
                    SyntaxFactory.ClassDeclaration(target.Name)
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.SealedKeyword),
                            SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                        .AddMembers(members)
                );

            var unit = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName("System")))
                .AddMembers(fileNs)
                .NormalizeWhitespace();

            spc.AddSource($"{target.Name}.g.cs",
                SourceText.From(unit.ToFullString(), Encoding.UTF8));
        }
    }

    private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol prop, bool required)
    {
        var xml = prop.GetDocumentationCommentXml() ?? "";
        var summary = Regex.Match(xml, "<summary>(.*?)</summary>",
                RegexOptions.Singleline)
            .Groups[1].Value.Trim();

        var decl = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(
                    prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                prop.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(
                        SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(
                        required
                            ? SyntaxKind.InitAccessorDeclaration
                            : SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(
                        SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        if (required)
            decl = decl.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.RequiredKeyword));

        if (!string.IsNullOrEmpty(summary))
        {
            decl = decl.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.Comment("/// <summary>"),
                    SyntaxFactory.Comment($"/// {summary}"),
                    SyntaxFactory.Comment("/// </summary>")));
        }

        return decl;
    }
}