using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf.Types;
using OneOf;
using Shared.Domain.Helpers;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Сообщение
/// </summary>
public sealed class Post : IHasCreateProperties, IHasUpdateProperties
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
    public PostContent Content { get; set; }

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

    public OneOf<Success, NonPostAuthorError, PostStaleError> Update(PostContent newContent, uint expectedRowVersion,
        UserId updateBy, DateTime updateAt)
    {
        if (CreatedBy != updateBy) return new NonPostAuthorError(ThreadId, PostId);

        if (RowVersion != expectedRowVersion) return new PostStaleError(ThreadId, PostId, RowVersion);

        Content = newContent;
        UpdatedBy = updateBy;
        UpdatedAt = updateAt;

        return OneOfHelper.Success;
    }
}