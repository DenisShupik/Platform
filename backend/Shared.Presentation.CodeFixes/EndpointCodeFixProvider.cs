using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Shared.Presentation.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EndpointCodeFixProvider)), Shared]
public sealed class EndpointCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("GP0102", "GP0106", "GP0107", "GP0113");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            switch (diagnostic.Id)
            {
                case "GP0113" when diagnostic.Properties.TryGetValue(
                    "SuggestedPattern",
                    out var suggestedPattern):
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Remove the leading slash",
                            cancellationToken => ReplaceRoutePatternAsync(
                                context.Document,
                                diagnostic,
                                _ => suggestedPattern!,
                                cancellationToken),
                            "GP0113.RemoveLeadingSlash"),
                        diagnostic);
                    break;

                case "GP0106" when
                    diagnostic.Properties.TryGetValue("RouteParameter", out var routeParameter) &&
                    diagnostic.Properties.TryGetValue(
                        "SuggestedRouteParameter",
                        out var suggestedRouteParameter) &&
                    !string.IsNullOrEmpty(suggestedRouteParameter):
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename route parameter to '{suggestedRouteParameter}'",
                            cancellationToken => ReplaceRoutePatternAsync(
                                context.Document,
                                diagnostic,
                                pattern => ReplaceRouteParameter(
                                    pattern,
                                    routeParameter!,
                                    suggestedRouteParameter!),
                                cancellationToken),
                            "GP0106.RenameRouteParameter"),
                        diagnostic);
                    break;

                case "GP0107" when diagnostic.Properties.TryGetValue(
                    "SuggestedPattern",
                    out var routeWithMissingParameter):
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Add the missing route parameter",
                            cancellationToken => ReplaceRoutePatternAsync(
                                context.Document,
                                diagnostic,
                                _ => routeWithMissingParameter!,
                                cancellationToken),
                            "GP0107.AddRouteParameter"),
                        diagnostic);
                    break;

                case "GP0102" when
                    diagnostic.Properties.TryGetValue("OperationKey", out var operationKey) &&
                    diagnostic.Properties.TryGetValue("DocumentationPath", out var documentationPath) &&
                    !string.IsNullOrEmpty(operationKey) &&
                    !string.IsNullOrEmpty(documentationPath):
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Add '{operationKey}' to Api.en.xml",
                            cancellationToken => AddDocumentationEntryAsync(
                                context.Document.Project,
                                documentationPath!,
                                operationKey!,
                                cancellationToken),
                            "GP0102.AddDocumentation"),
                        diagnostic);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> ReplaceRoutePatternAsync(
        Document document,
        Diagnostic diagnostic,
        Func<string, string> replace,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var expression = node.FirstAncestorOrSelf<ExpressionSyntax>();
        if (expression is null) return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var value = semanticModel?.GetConstantValue(expression, cancellationToken);
        if (value is not { HasValue: true, Value: string pattern }) return document;

        var replacement = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(replace(pattern)))
            .WithTriviaFrom(expression);
        return document.WithSyntaxRoot(root.ReplaceNode(expression, replacement));
    }

    private static string ReplaceRouteParameter(
        string pattern,
        string routeParameter,
        string suggestedRouteParameter)
    {
        for (var index = 0; index < pattern.Length; index++)
        {
            if (pattern[index] != '{') continue;

            var nameStart = index + 1;
            while (nameStart < pattern.Length && pattern[nameStart] == '*') nameStart++;
            var nameEnd = nameStart;
            while (nameEnd < pattern.Length &&
                   pattern[nameEnd] is not ('}' or ':' or '=' or '?'))
                nameEnd++;

            if (string.Equals(
                    pattern.Substring(nameStart, nameEnd - nameStart),
                    routeParameter,
                    StringComparison.OrdinalIgnoreCase))
                return pattern.Substring(0, nameStart) +
                       suggestedRouteParameter +
                       pattern.Substring(nameEnd);
        }

        return pattern;
    }

    private static async Task<Solution> AddDocumentationEntryAsync(
        Project project,
        string documentationPath,
        string operationKey,
        CancellationToken cancellationToken)
    {
        var normalizedPath = Path.GetFullPath(documentationPath);
        var document = project.AdditionalDocuments.FirstOrDefault(candidate =>
            candidate.FilePath is not null &&
            string.Equals(
                Path.GetFullPath(candidate.FilePath),
                normalizedPath,
                StringComparison.OrdinalIgnoreCase));
        if (document is null) return project.Solution;

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var closingElement = text.ToString().LastIndexOf("</docs>", StringComparison.Ordinal);
        if (closingElement < 0) return project.Solution;

        var newLine = text.ToString().Contains("\r\n") ? "\r\n" : "\n";
        var entry =
            $"  <operation key=\"{operationKey}\"><summary>TODO: describe {operationKey}</summary></operation>{newLine}";
        var changedText = text.WithChanges(new TextChange(new TextSpan(closingElement, 0), entry));
        return project.Solution.WithAdditionalDocumentText(document.Id, changedText);
    }
}
