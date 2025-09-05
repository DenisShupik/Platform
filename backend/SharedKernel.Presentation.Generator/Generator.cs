using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SharedKernel.Presentation.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is PropertyDeclarationSyntax { AttributeLists.Count: > 0, Initializer: not null },
                transform: static (ctx, _) =>
                {
                    var propDecl = (PropertyDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;
                    var propSymbol = ModelExtensions.GetDeclaredSymbol(model, propDecl) as IPropertySymbol;
                    if (propSymbol == null) return null;

                    // Проверяем наличие атрибута Optional или OptionalAttribute
                    var hasOptional = propSymbol.GetAttributes().Any(a =>
                        string.Equals(a.AttributeClass?.Name, "OptionalAttribute", StringComparison.Ordinal) ||
                        string.Equals(a.AttributeClass?.Name, "Optional", StringComparison.Ordinal));

                    if (!hasOptional) return null;

                    // Только nullable свойства (которые действительно могут быть null)
                    if (propSymbol.NullableAnnotation != NullableAnnotation.Annotated) return null;

                    if (propDecl.Initializer == null) return null;

                    // Получим синтаксис инициализатора
                    var initExprSyntax = propDecl.Initializer.Value;

                    // Попробуем получить семантику и, если это обращение к статическому/члену типа, построить fully-qualified с global::
                    string initializerReplacement;
                    try
                    {
                        var symbolInfo = model.GetSymbolInfo(initExprSyntax);
                        var memberSymbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                        if (memberSymbol != null && memberSymbol.ContainingType != null)
                        {
                            // Пример: global::Namespace.ContainingType.MemberName
                            var containingTypeFqn = memberSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            // Т.к. ToDisplayString(FullyQualifiedFormat) обычно возвращает "global::..." уже — но на разных версиях Roslyn
                            // безопасно нормализовать: если там уже есть "global::", не добавляем второй раз.
                            if (containingTypeFqn.StartsWith("global::", StringComparison.Ordinal))
                                initializerReplacement = $"{containingTypeFqn}.{memberSymbol.Name}";
                            else
                                initializerReplacement = $"global::{containingTypeFqn}.{memberSymbol.Name}";
                        }
                        else
                        {
                            // Фоллбек — исходный текст инициализатора
                            initializerReplacement = initExprSyntax.ToFullString();
                        }
                    }
                    catch
                    {
                        // В крайнем случае используем исходный текст
                        initializerReplacement = initExprSyntax.ToFullString();
                    }

                    // Сохраним нужные данные: символ, текст (заменённый / оригинальный) и оригинальный синтаксис
                    return new PropInfo(
                        propSymbol,
                        initializerReplacement,
                        propDecl);
                })
            .Where(x => x != null)
            .Collect();

        context.RegisterSourceOutput(candidates, (spc, list) =>
        {
            var items = list.Where(x => x != null).Cast<PropInfo>().ToArray();
            if (items.Length == 0) return;

            // Группируем по содержащему типу — используем ISymbol + SymbolEqualityComparer.Default
            var groupsByISymbol = items.GroupBy(i => (ISymbol)i.Symbol.ContainingType, SymbolEqualityComparer.Default);

            foreach (var grp in groupsByISymbol)
            {
                if (grp.Key is not INamedTypeSymbol containingType) continue;

                var ns = containingType.ContainingNamespace is { IsGlobalNamespace: false }
                    ? containingType.ContainingNamespace.ToDisplayString()
                    : null;

                var cu = GenerateCompilationUnit(ns, containingType, grp.ToArray());
                var sourceText = SourceText.From(cu.NormalizeWhitespace().ToFullString(), Encoding.UTF8);

                var hint = $"{containingType.Name}_OptionalDefaults.g.cs";
                spc.AddSource(hint, sourceText);
            }
        });
    }

    private CompilationUnitSyntax GenerateCompilationUnit(string? ns, INamedTypeSymbol containingType, PropInfo[] props)
    {
        // Заголовок авто-сгенерированного файла
        var headerTrivia = SyntaxFactory.TriviaList(
            SyntaxFactory.Comment("// <auto-generated />"),
            SyntaxFactory.CarriageReturnLineFeed
        );

        // Сгенерировать members (свойства) для типа
        var propertyMembers = props.Select(GenerateGetterProperty).ToArray();

        // Построим цепочку вложенных типов (outer -> inner)
        // Сначала создаём declaration для самого внутреннего типа с нашими свойствами
        var innerTypeDecl = CreateTypeDeclaration(containingType, propertyMembers);

        // Если тип вложен, завернём в оболочки для внешних типов
        var outer = innerTypeDecl;
        var current = containingType.ContainingType;
        while (current != null)
        {
            // Создадим пустой partial-тип с одним членом — предыдущим outer
            var wrapper = CreateTypeDeclaration(current, new MemberDeclarationSyntax[] { outer });
            outer = wrapper;
            current = current.ContainingType;
        }

        MemberDeclarationSyntax topMember = outer;

        CompilationUnitSyntax cu;
        if (!string.IsNullOrEmpty(ns))
        {
            var namespaceDecl = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns))
                .WithMembers(SyntaxFactory.SingletonList(topMember));
            cu = SyntaxFactory.CompilationUnit()
                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceDecl))
                .WithLeadingTrivia(headerTrivia);
        }
        else
        {
            cu = SyntaxFactory.CompilationUnit()
                .WithMembers(SyntaxFactory.SingletonList(topMember))
                .WithLeadingTrivia(headerTrivia);
        }

        return cu;
    }

    private static TypeDeclarationSyntax CreateTypeDeclaration(INamedTypeSymbol typeSymbol, MemberDeclarationSyntax[] members)
    {
        // class/struct
        var isStruct = typeSymbol.TypeKind == TypeKind.Struct;

        var typeName = typeSymbol.Name;

        // Generic type parameters
        TypeParameterListSyntax? typeParams = null;
        if (typeSymbol.TypeParameters.Length > 0)
        {
            typeParams = SyntaxFactory.TypeParameterList(
                SyntaxFactory.SeparatedList(typeSymbol.TypeParameters.Select(tp => SyntaxFactory.TypeParameter(tp.Name)))
            );
        }

        // Modifiers: accessibility + partial
        var mods = SyntaxFactory.TokenList();
        mods = mods.Add(SyntaxFactory.Token(AccessibilityToSyntaxKind(typeSymbol.DeclaredAccessibility)));
        mods = mods.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        TypeDeclarationSyntax decl = isStruct
            ? SyntaxFactory.StructDeclaration(typeName)
            : SyntaxFactory.ClassDeclaration(typeName);

        decl = decl.WithModifiers(mods);

        if (typeParams != null)
            decl = decl.WithTypeParameterList(typeParams);

        // Добавим членов
        decl = decl.WithMembers(SyntaxFactory.List(members));

        return decl;
    }

    private static MemberDeclarationSyntax GenerateGetterProperty(PropInfo p)
    {
        var propSymbol = p.Symbol;
        var propName = propSymbol.Name;
        var getterName = "Get" + propName;

        // Return type: если Nullable<T> (value-type), используем T, иначе используем сам тип
        var returnTypeName = GetReturnTypeString(propSymbol);

        // Parse initializer expression text (берём подготовленный синтаксический текст — возможно fully-qualified)
        var initExpr = SyntaxFactory.ParseExpression(p.InitializerText);

        // Build coalesce expression: <PropName> ?? <initializer>
        var left = SyntaxFactory.IdentifierName(propName);
        var coalesce = SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, left, initExpr);

        // Property: public <type> GetXxx => PropName ?? Initializer;
        var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(returnTypeName), SyntaxFactory.Identifier(getterName))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(coalesce))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(AccessibilityToSyntaxKind(propSymbol.DeclaredAccessibility))));

        return property;
    }

    private static string GetReturnTypeString(IPropertySymbol propSymbol)
    {
        var t = propSymbol.Type;

        if (t is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && named.TypeArguments.Length == 1)
        {
            // Nullable<T> (value type) -> T
            return named.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        // Для ссылочных типов с nullable-аннотацией ToDisplayString не добавляет '?', но нам нужен ненулевой вариант — используем сам тип
        return t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static SyntaxKind AccessibilityToSyntaxKind(Accessibility a) =>
        a switch
        {
            Accessibility.Public => SyntaxKind.PublicKeyword,
            Accessibility.Internal => SyntaxKind.InternalKeyword,
            Accessibility.Private => SyntaxKind.PrivateKeyword,
            Accessibility.Protected => SyntaxKind.ProtectedKeyword,
            Accessibility.ProtectedAndInternal => SyntaxKind.PrivateKeyword, // private protected
            Accessibility.ProtectedOrInternal => SyntaxKind.ProtectedKeyword, // protected internal (approx)
            _ => SyntaxKind.PrivateKeyword
        };

    private record PropInfo(IPropertySymbol Symbol, string InitializerText, PropertyDeclarationSyntax OriginalDecl)
    {
        public IPropertySymbol Symbol { get; } = Symbol;
        public string InitializerText { get; } = InitializerText;
        public PropertyDeclarationSyntax OriginalDecl { get; } = OriginalDecl;
    }
}
