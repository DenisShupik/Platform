using CoreService.Domain.Enums;

namespace CoreService.Domain.ValueObjects;

/// <summary>
/// Неизменяемая ресурсная область. Неконсистентное сочетание уровня и идентификаторов создать нельзя.
/// </summary>
public sealed record AuthorizationScope
{
    public AuthorizationScopeType Type { get; }
    public ForumId? ForumId { get; }
    public CategoryId? CategoryId { get; }
    public ThreadId? ThreadId { get; }

    private AuthorizationScope(
        AuthorizationScopeType type,
        ForumId? forumId,
        CategoryId? categoryId,
        ThreadId? threadId)
    {
        Type = type;
        ForumId = forumId;
        CategoryId = categoryId;
        ThreadId = threadId;
    }

    public static AuthorizationScope Platform { get; } =
        new(AuthorizationScopeType.Platform, null, null, null);

    public static AuthorizationScope Forum(ForumId forumId) =>
        new(AuthorizationScopeType.Forum, forumId, null, null);

    public static AuthorizationScope Category(ForumId forumId, CategoryId categoryId) =>
        new(AuthorizationScopeType.Category, forumId, categoryId, null);

    public static AuthorizationScope Thread(ForumId forumId, CategoryId categoryId, ThreadId threadId) =>
        new(AuthorizationScopeType.Thread, forumId, categoryId, threadId);
}
