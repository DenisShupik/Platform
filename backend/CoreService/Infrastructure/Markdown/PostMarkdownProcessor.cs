using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Options;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;

namespace CoreService.Infrastructure.Markdown;

public sealed class PostMarkdownProcessor : IPostContentPolicy, IPostSearchTextProjector
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

    public bool IsAllowed(PostContent content)
    {
        var document = Markdig.Markdown.Parse(content.Value, Pipeline);

        return !document.Descendants<HtmlBlock>().Any() &&
               !document.Descendants<HtmlInline>().Any() &&
               document.Descendants<LinkInline>().All(IsAllowedLink) &&
               !string.IsNullOrWhiteSpace(ToPlainText(content));
    }

    public string Project(PostContent content) => ToPlainText(content);

    private static string ToPlainText(PostContent content) =>
        Markdig.Markdown.ToPlainText(content.Value, Pipeline).Trim();

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
