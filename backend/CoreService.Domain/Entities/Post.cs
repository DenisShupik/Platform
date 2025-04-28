using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf.Types;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;
using OneOf;
using SharedKernel.Domain.Helpers;

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
    /// Дата и время создания сообщения
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения сообщения
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего сообщение
    /// </summary>
    public UserId UpdatedBy { get; private set; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; private set; }

    internal Post(ThreadId threadId, PostId postId, PostContent content, UserId createdBy, DateTime createdAt)
    {
        ThreadId = threadId;
        PostId = postId;
        Content = content;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = createdBy;
        UpdatedAt = createdAt;
    }

    public OneOf<NonPostAuthorError, PostStaleError, Success> Update(PostContent newContent, uint expectedRowVersion,
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