using CoreService.Domain.Errors;
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
    public RowVersion RowVersion { get; private set; }

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

    internal static Post Create(
        ThreadId threadId,
        PostContent content,
        UserId createdBy,
        DateTime createdAt) => new(threadId, content, createdBy, createdAt);

    internal Result<Success, PostStaleError> UpdateContent(
        PostContent newContent,
        RowVersion expectedRowVersion,
        UserId updatedBy,
        DateTime updatedAt)
    {
        if (RowVersion != expectedRowVersion) return new PostStaleError(ThreadId, PostId, RowVersion);

        Content = newContent;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;

        return Success.Instance;
    }
}
