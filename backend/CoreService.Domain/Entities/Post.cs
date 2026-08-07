using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Сообщение
/// </summary>
public sealed class Post
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public PostId PostId { get; private set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; private set; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public PostContent Content { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего сообщение
    /// </summary>
    public UserId UpdatedBy { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения сообщения
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; private set; }

    private Post(ThreadId threadId, PostContent content, UserId createdBy, DateTime createdAt)
    {
        PostId = PostId.From(Guid.CreateVersion7());
        ThreadId = threadId;
        Content = content;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = createdBy;
        UpdatedAt = createdAt;
    }

    internal static Result<Post, InvalidPostContentError> Create(
        ThreadId threadId,
        PostContent content,
        UserId createdBy,
        DateTime createdAt,
        IPostContentPolicy postContentPolicy)
    {
        if (!HasAllowedContent(content, postContentPolicy)) return new InvalidPostContentError();

        return new Post(threadId, content, createdBy, createdAt);
    }

    internal Result<Success, PostStaleError, InvalidPostContentError> UpdateContent(
        PostContent newContent,
        uint expectedRowVersion,
        UserId updatedBy,
        DateTime updatedAt,
        IPostContentPolicy postContentPolicy)
    {
        if (RowVersion != expectedRowVersion) return new PostStaleError(ThreadId, PostId, RowVersion);
        if (!HasAllowedContent(newContent, postContentPolicy)) return new InvalidPostContentError();

        Content = newContent;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;

        return Success.Instance;
    }

    private static bool HasAllowedContent(PostContent content, IPostContentPolicy postContentPolicy) =>
        postContentPolicy.IsAllowed(content);
}
