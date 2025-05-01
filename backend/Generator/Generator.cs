using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generator;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor PropertyTypeMismatch = new(
        id: "CRG001",
        title: "Property type mismatch",
        messageFormat: "Type in typeof({0}) does not match type in nameof({1}.{2})",
        category: nameof(Generator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        (node, _) => node is ClassDeclarationSyntax cds
                            && cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                            && cds.AttributeLists.Count > 0,
                        (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                    .Where(cd => cd is not null)
                    .Collect()),
            static (spc, source) => Execute(spc, source.Left, source.Right));
    }

    private static void Execute(SourceProductionContext spc, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var omitSym  = compilation.GetTypeByMetadataName("Generator.Attributes.OmitAttribute");
        var asReqSym = compilation.GetTypeByMetadataName("Generator.Attributes.IncludeAsRequiredAttribute");
        if (omitSym is null || asReqSym is null) return;

        foreach (var classDecl in classes)
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (model.GetDeclaredSymbol(classDecl) is not { } target) continue;

            var entries = ImmutableArray.CreateBuilder<(INamedTypeSymbol Source, string Name, bool Required)>();
            var omitAttrs = target.GetAttributes()
                .Where(attr => attr.AttributeClass is not null &&
                               SymbolEqualityComparer.Default.Equals(attr.AttributeClass, omitSym))
                .ToArray();

            // Проверка nameof(SomeType.Property) на соответствие typeof(...)
            foreach (var attr in classDecl.AttributeLists.SelectMany(al => al.Attributes))
            {
                var attrSymbol = model.GetSymbolInfo(attr).Symbol?.ContainingType;
                if (attrSymbol is null ||
                    (!SymbolEqualityComparer.Default.Equals(attrSymbol, asReqSym) &&
                     !SymbolEqualityComparer.Default.Equals(attrSymbol, omitSym)))
                    continue;

                var args = attr.ArgumentList?.Arguments;
                if (args is null || args.Value.Count < 2) continue;

                var typeInfo = args.Value[0].Expression is TypeOfExpressionSyntax typeOfExpr
                    ? model.GetTypeInfo(typeOfExpr.Type).Type as INamedTypeSymbol
                    : null;
                if (typeInfo is null) continue;

                for (var i = 1; i < args.Value.Count; i++)
                {
                    if (args.Value[i].Expression is not InvocationExpressionSyntax
                        {
                            Expression: IdentifierNameSyntax { Identifier.Text: "nameof" },
                            ArgumentList.Arguments.Count: > 0
                        } invocation
                        || invocation.ArgumentList.Arguments[0].Expression is not MemberAccessExpressionSyntax memberAccess)
                        continue;

                    if (model.GetTypeInfo(memberAccess.Expression).Type is INamedTypeSymbol leftType &&
                        !SymbolEqualityComparer.Default.Equals(leftType, typeInfo))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(
                            PropertyTypeMismatch,
                            args.Value[i].GetLocation(),
                            typeInfo.ToDisplayString(),
                            leftType.ToDisplayString(),
                            memberAccess.Name.Identifier.Text));
                    }
                }
            }

            // IncludeAsRequired (старое поведение)
            foreach (var attr in target.GetAttributes())
            {
                var cls = attr.AttributeClass;
                if (cls is null ||
                    !SymbolEqualityComparer.Default.Equals(cls, asReqSym))
                    continue;
                
                if (attr.ConstructorArguments.Length < 2
                    || attr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType)
                    continue;

                foreach (var name in attr.ConstructorArguments[1].Values.Select(v => v.Value as string).Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var propSym = srcType.GetMembers(name!)
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

                    entries.Add((srcType, name!, true));
                }
            }

            // OmitAttribute: копируем все свойства, кроме указанных
            foreach (var omitAttr in omitAttrs)
            {
                if (omitAttr.ConstructorArguments.Length < 2
                    || omitAttr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType)
                    continue;

                var omitted = omitAttr.ConstructorArguments[1].Values
                    .Select(v => v.Value as string)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToImmutableHashSet();

                var allProps = srcType.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !omitted.Contains(p.Name))
                    .ToArray();

                foreach (var prop in allProps)
                {
                    // Не required, обычная генерация
                    entries.Add((srcType, prop.Name, false));
                }
            }

            if (entries.Count == 0) continue;

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

            var ns = target.ContainingNamespace?.ToDisplayString() ?? "Generated";
            var classSyntax = SyntaxFactory.ClassDeclaration(target.Name)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.SealedKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(members);

            var nsSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns))
                .AddMembers(classSyntax);

            var unit = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(nsSyntax)
                .NormalizeWhitespace();

            spc.AddSource($"{target.Name}.g.cs", SourceText.From(unit.ToFullString(), Encoding.UTF8));
        }
    }

    private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol prop, bool required)
    {
        var summary = Regex.Match(prop.GetDocumentationCommentXml() ?? string.Empty,
            "<summary>(.*?)</summary>", RegexOptions.Singleline).Groups[1].Value.Trim();

        var decl = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                prop.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(required
                        ? SyntaxKind.InitAccessorDeclaration
                        : SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        if (required)
            decl = decl.AddModifiers(SyntaxFactory.Token(SyntaxKind.RequiredKeyword));

        return string.IsNullOrEmpty(summary)
            ? decl
            : decl.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.Comment("/// <summary>"),
                    SyntaxFactory.Comment($"/// {summary}"),
                    SyntaxFactory.Comment("/// </summary>")));
    }
}