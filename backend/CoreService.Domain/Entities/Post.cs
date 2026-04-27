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
    public PostContent Content { get; internal set; }

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
    public UserId UpdatedBy { get; internal set; }

    /// <summary>
    /// Дата и время последнего изменения сообщения
    /// </summary>
    public DateTime UpdatedAt { get; internal set; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; private set; }

    internal Post(ThreadId threadId, PostContent content, UserId createdBy, DateTime createdAt)
    {
        PostId = PostId.From(Guid.CreateVersion7());
        ThreadId = threadId;
        Content = content;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = createdBy;
        UpdatedAt = createdAt;
    }

    public Result<Success, PostStaleError> UpdatePost(PostContent newContent, uint expectedRowVersion, UserId updatedBy,
        DateTime updatedAt)
    {
        if (RowVersion != expectedRowVersion) return new PostStaleError(ThreadId, PostId, RowVersion);

        Content = newContent;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;

        return Success.Instance;
    }
}