using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class ApiTagDescriptionDocumentTransformer : IOpenApiDocumentTransformer
{
    private static readonly IReadOnlyDictionary<string, string> Descriptions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ForumApi"] = "Forum administration and discovery.",
            ["AuthorizationApi"] = "Capability assignments, administrative appointments, and forum sanctions.",
            ["CategoryApi"] = "Forum category administration and discovery.",
            ["ThreadApi"] = "Thread lifecycle, moderation, and reading.",
            ["PostApi"] = "Post lifecycle and reading.",
            ["PostBookmarkApi"] = "Bookmarks managed by the current user.",
            ["UserBookmarkApi"] = "Public bookmark lists associated with users.",
            ["SearchApi"] = "Forum content search.",
            ["AvatarApi"] = "Avatar management for the current user.",
            ["InternalNotificationApi"] = "The current user's internal notification inbox.",
            ["UserSubscriptionApi"] = "Thread subscriptions managed by the current user.",
            ["UserApi"] = "User profiles and preferences."
        };

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Tags is null) return Task.CompletedTask;

        foreach (var tag in document.Tags)
        {
            if (tag.Name is not null && Descriptions.TryGetValue(tag.Name, out var description))
                tag.Description = description;
        }

        return Task.CompletedTask;
    }
}
