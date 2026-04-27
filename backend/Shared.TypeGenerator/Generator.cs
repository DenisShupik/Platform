using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Shared.TypeGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private const string AttributesNamespace = "Shared.TypeGenerator.Attributes";

    // Определяем константы для режимов генерации
    private const int PropertyGenerationModeAsPrivateSet = 0;
    private const int PropertyGenerationModeAsPublic = 1;
    private const int PropertyGenerationModeAsRequired = 2;

    private static readonly DiagnosticDescriptor InternalError = new(
        id: "STG000",
        title: "Source generator internal error",
        messageFormat: "An unexpected exception occurred inside the source generator: {0}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PropertyTypeMismatch = new(
        id: "STG001",
        title: "Property type mismatch",
        messageFormat: "Type in typeof({0}) does not match type in nameof({1}.{2})",
        category: nameof(Generator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor PropertyNotFound = new(
        id: "STG002",
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
            static (spc, source) =>
            {
                try
                {
                    Execute(spc, source.Left, source.Right);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(InternalError, null, exception.Message));
                }
            });
    }

    private static void Execute(
        SourceProductionContext spc,
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes)
    {
        var omitSym = compilation.GetTypeByMetadataName($"{AttributesNamespace}.OmitAttribute");
        var includeSym = compilation.GetTypeByMetadataName($"{AttributesNamespace}.IncludeAttribute");
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

                for (var i = 2; i < args.Value.Count; i++)
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
                    if (name == null) continue;
                    // ИЗМЕНЕНИЕ: Используем GetAllProperties для поиска по всей иерархии
                    var propSym = GetAllProperties(srcType).FirstOrDefault(p => p.Name == name);

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
                    if (includeAttr.ConstructorArguments.Length < 3
                        || includeAttr.ConstructorArguments[0].Value is not INamedTypeSymbol includeSource)
                        continue;

                    foreach (var name in includeAttr.ConstructorArguments[2].Values
                                 .Select(nameObj => nameObj.Value as string)
                                 .Where(name => !string.IsNullOrWhiteSpace(name)))
                    {
                        if (name == null || omitted.Contains(name)) continue;
                        if (!addedProperties.Add(name)) continue;
                        // ИЗМЕНЕНИЕ: Используем GetAllProperties для поиска по всей иерархии
                        var prop = GetAllProperties(includeSource).FirstOrDefault(p => p.Name == name);
                        if (prop != null)
                        {
                            entries.Add((includeSource, name, modeValue));
                        }
                    }
                }

                // Затем добавляем свойства из самого класса и его базовых классов
                // ИЗМЕНЕНИЕ: Используем GetAllProperties для обхода всей иерархии
                foreach (var prop in GetAllProperties(srcType))
                {
                    if (omitted.Contains(prop.Name)) continue;
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
                // ИЗМЕНЕНИЕ: Используем GetAllProperties для финального поиска символа
                var propSym = GetAllProperties(entry.Source).FirstOrDefault(p => p.Name == entry.Name);

                if (propSym != null)
                {
                    members.Add(CreateProperty(propSym, entry.Mode));
                }
            }

            if (members.Count == 0)
                continue;

            var nsName = target.ContainingNamespace?.ToDisplayString() ?? "Generated";
            var fileNs = FileScopedNamespaceDeclaration(ParseName(nsName))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken)
                        .WithTrailingTrivia(LineFeed))
                .AddMembers(
                    ClassDeclaration(target.Name)
                        .AddModifiers(Token(SyntaxKind.PartialKeyword))
                        .WithTypeParameterList(!target.TypeParameters.IsDefaultOrEmpty
                            ? TypeParameterList(
                                SeparatedList(target.TypeParameters.Select(tp => TypeParameter(tp.Name))))
                            : null)
                        .AddMembers(members.ToArray())
                );


            var unit = CompilationUnit()
                    .AddUsings(UsingDirective(ParseName("System")))
                    .AddMembers(fileNs)
                    .WithLeadingTrivia(
                        TriviaList(
                            Comment("// <auto-generated />"),
                            Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), isActive: true))
                        )
                    )
                    .NormalizeWhitespace()
                ;

            spc.AddSource($"{target.Name}.g.cs",
                SourceText.From(unit.ToFullString(), Encoding.UTF8));
        }
    }

    // НОВЫЙ МЕТОД
    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
    {
        var names = new HashSet<string>();
        var currentType = type;
        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            foreach (var prop in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                if (names.Add(prop.Name))
                {
                    yield return prop;
                }
            }

            currentType = currentType.BaseType;
        }
    }

    private static PropertyDeclarationSyntax CreateProperty(IPropertySymbol prop, int mode)
    {
        var xml = prop.GetDocumentationCommentXml() ?? "";
        var summary = Regex.Match(xml, "<summary>(.*?)</summary>",
                RegexOptions.Singleline)
            .Groups[1].Value.Trim();

        var decl = PropertyDeclaration(
            ParseTypeName(
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
                TriviaList(
                    Comment("/// <summary>"),
                    Comment($"/// {summary}"),
                    Comment("/// </summary>")));
        }

        return decl;
    }

    private static PropertyDeclarationSyntax CreateAsPrivateSetProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(Token(SyntaxKind.PublicKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SeparatedList(accessors))));

        return decl;
    }

    private static PropertyDeclarationSyntax CreateAsPublicProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(Token(SyntaxKind.PublicKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SeparatedList(accessors))));

        return decl;
    }

    private static PropertyDeclarationSyntax CreateAsRequiredProperty(PropertyDeclarationSyntax decl,
        IPropertySymbol prop)
    {
        decl = decl.AddModifiers(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.RequiredKeyword));

        var accessors = new List<AccessorDeclarationSyntax>
        {
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
            AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        };

        decl = decl.WithAccessorList(
            AccessorList(
                new SyntaxList<AccessorDeclarationSyntax>(SeparatedList(accessors))));

        return decl;
    }
}