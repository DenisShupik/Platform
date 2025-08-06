using System.Collections.Generic;
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
    // Определяем константы для режимов генерации
    private const int PropertyGenerationModeAsPrivateSet = 0;
    private const int PropertyGenerationModeAsPublic = 1;
    private const int PropertyGenerationModeAsRequired = 2;

    private static readonly DiagnosticDescriptor PropertyTypeMismatch = new(
        id: "CRG001",
        title: "Property type mismatch",
        messageFormat: "Type in typeof({0}) does not match type in nameof({1}.{2})",
        category: nameof(Generator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor PropertyNotFound = new(
        id: "CRG002",
        title: "Property not found",
        messageFormat: "Property '{0}' not found in type '{1}'",
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
        var includeSym = compilation.GetTypeByMetadataName("Generator.Attributes.IncludeAttribute");
        if (omitSym is null || includeSym is null) return;

        foreach (var classDecl in classes)
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (model.GetDeclaredSymbol(classDecl) is not { } target) continue;

            var addedProperties = new HashSet<string>(); // Отслеживаем добавленные свойства
            var entries = ImmutableArray.CreateBuilder<(INamedTypeSymbol Source, string Name, int Mode)>();

            var omitAttrs = target.GetAttributes()
                .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, omitSym))
                .ToArray();

            // Валидация синтаксиса
            foreach (var attr in classDecl.AttributeLists.SelectMany(al => al.Attributes))
            {
                var attrSym = model.GetSymbolInfo(attr).Symbol?.ContainingType;
                if (attrSym is null ||
                    (!SymbolEqualityComparer.Default.Equals(attrSym, omitSym) &&
                     !SymbolEqualityComparer.Default.Equals(attrSym, includeSym)))
                    continue;

                var args = attr.ArgumentList?.Arguments;
                if (args is null || args.Value.Count < 3) continue;

                var typeInfo = args.Value[0].Expression is TypeOfExpressionSyntax typeOfExpr
                    ? model.GetTypeInfo(typeOfExpr.Type).Type as INamedTypeSymbol
                    : null;
                if (typeInfo is null) continue;

                for (int i = 2; i < args.Value.Count; i++)
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

            // Обрабатываем Include атрибуты
            foreach (var attr in target.GetAttributes()
                         .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, includeSym)))
            {
                if (attr.ConstructorArguments.Length < 3
                    || attr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType
                    || attr.ConstructorArguments[1].Value is not int modeValue)
                    continue;

                foreach (var name in attr.ConstructorArguments[2].Values
                             .Select(nameObj => nameObj.Value as string)
                             .Where(name => !string.IsNullOrWhiteSpace(name)))
                {
                    if (name != null)
                    {
                        var propSym = srcType.GetMembers(name)
                            .OfType<IPropertySymbol>()
                            .FirstOrDefault();

                        if (propSym is null)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                PropertyNotFound,
                                classDecl.GetLocation(),
                                name,
                                srcType.ToDisplayString()));
                            continue;
                        }

                        // Добавляем только если свойство еще не было добавлено
                        if (addedProperties.Add(name))
                        {
                            entries.Add((srcType, name, modeValue));
                        }
                    }
                }
            }

            // Обрабатываем Omit атрибуты
            foreach (var omitAttr in omitAttrs)
            {
                if (omitAttr.ConstructorArguments.Length < 2
                    || omitAttr.ConstructorArguments[0].Value is not INamedTypeSymbol srcType
                    || omitAttr.ConstructorArguments[1].Value is not int modeValue)
                    continue;

                var omitted = omitAttr.ConstructorArguments.Length >= 3
                    ? omitAttr.ConstructorArguments[2].Values
                        .Select(v => v.Value as string)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToImmutableHashSet()
                    : ImmutableHashSet<string>.Empty;

                // Сначала добавляем свойства из Include атрибутов исходного типа
                foreach (var includeAttr in srcType.GetAttributes()
                             .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, includeSym)))
                {
                    // Заменяем pattern matching на обычную проверку
                    if (includeAttr.ConstructorArguments.Length < 3
                        || includeAttr.ConstructorArguments[0].Value is not INamedTypeSymbol includeSource)
                        continue;

                    foreach (var name in includeAttr.ConstructorArguments[2].Values
                                 .Select(nameObj => nameObj.Value as string)
                                 .Where(name => !string.IsNullOrWhiteSpace(name)))
                    {
                        if (name == null || omitted.Contains(name)) continue;
                        // Добавляем только если свойство еще не было добавлено
                        if (!addedProperties.Add(name)) continue;
                        var prop = includeSource.GetMembers(name).OfType<IPropertySymbol>()
                            .FirstOrDefault();
                        if (prop != null)
                        {
                            entries.Add((includeSource, name, modeValue));
                        }
                    }
                }

                // Затем добавляем свойства из самого класса
                foreach (var prop in srcType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (omitted.Contains(prop.Name)) continue;
                    // Добавляем только если свойство еще не было добавлено
                    if (addedProperties.Add(prop.Name))
                    {
                        entries.Add((srcType, prop.Name, modeValue));
                    }
                }
            }

            if (entries.Count == 0)
                continue;

            // Собираем свойства из всех источников
            var members = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();

            foreach (var entry in entries)
            {
                var propSym = entry.Source.GetMembers(entry.Name)
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault();

                if (propSym != null)
                {
                    members.Add(CreateProperty(propSym, entry.Mode));
                }
            }

            if (members.Count == 0)
                continue;

            var nsName = target.ContainingNamespace?.ToDisplayString() ?? "Generated";
            var fileNs = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(nsName))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                        .WithTrailingTrivia(SyntaxFactory.LineFeed))
                .AddMembers(
                    SyntaxFactory.ClassDeclaration(target.Name)
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                        .AddMembers(members.ToArray())
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

    private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol prop, int mode)
    {
        var xml = prop.GetDocumentationCommentXml() ?? "";
        var summary = Regex.Match(xml, "<summary>(.*?)</summary>",
                RegexOptions.Singleline)
            .Groups[1].Value.Trim();

        var decl = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(
                prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
            prop.Name);

        switch (mode)
        {
            case PropertyGenerationModeAsPrivateSet:
                decl = CreateAsPrivateSetProperty(decl, prop);
                break;

            case PropertyGenerationModeAsPublic:
                decl = CreateAsPublicProperty(decl, prop);
                break;

            case PropertyGenerationModeAsRequired:
                decl = CreateAsRequiredProperty(decl, prop);
                break;
        }

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

    private static PropertyDeclarationSyntax CreateAsPrivateSetProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            SyntaxFactory.AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SyntaxFactory.SeparatedList(accessors))));

        return decl;
    }

    private static PropertyDeclarationSyntax CreateAsPublicProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            SyntaxFactory.AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SyntaxFactory.SeparatedList(accessors))));

        return decl;
    }

    private static PropertyDeclarationSyntax CreateAsRequiredProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.RequiredKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            SyntaxFactory.AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SyntaxFactory.SeparatedList(accessors))));

        return decl;
    }
}