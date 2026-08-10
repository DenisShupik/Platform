using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Options;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Infrastructure.Markdown;

public sealed class PostMarkdownProcessor : IPostContentProcessor
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly HashSet<string> _allowedLinkHosts;

    public PostMarkdownProcessor(IOptions<PostContentPolicyOptions> options)
    {
        _allowedLinkHosts = (options.Value.AllowedLinkHosts ?? [])
            .Select(static host => host.TrimEnd('.').ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }

    public Result<ProcessedPostContent, InvalidPostContentError> Process(PostContent content)
    {
        var document = Markdig.Markdown.Parse(content.Value, Pipeline);

        foreach (var descendant in document.Descendants())
        {
            if (descendant is HtmlBlock or HtmlInline ||
                descendant is LinkInline link && !IsAllowedLink(link))
                return new InvalidPostContentError();
        }

        var searchText = ToPlainText(document);
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length > PostContent.MaxLength)
            return new InvalidPostContentError();

        return new ProcessedPostContent(content, searchText);
    }

    private static string ToPlainText(MarkdownDocument document)
    {
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer)
        {
            EnableHtmlForBlock = false,
            EnableHtmlForInline = false,
            EnableHtmlEscape = false
        };
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        return writer.ToString().Trim();
    }

    private bool IsAllowedLink(LinkInline link)
    {
        if (string.IsNullOrWhiteSpace(link.Url)) return false;

        var url = link.Url;
        if (url.StartsWith("//", StringComparison.Ordinal)) return false;

        if (Uri.TryCreate(url, UriKind.Relative, out _)) return true;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (!uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(uri.UserInfo)) return false;

        return _allowedLinkHosts.Count == 0 || _allowedLinkHosts.Contains(uri.IdnHost);
    }
}
