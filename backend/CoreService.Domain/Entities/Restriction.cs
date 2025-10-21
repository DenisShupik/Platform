using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Ограничение
/// </summary>
[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
[Include(typeof(Policy), PropertyGenerationMode.AsPrivateSet, nameof(Policy.Type))]
public abstract partial class Restriction : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор пользователя, создавшего ограничение
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания ограничение
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время окончания действия ограничения (если <c>null</c>, то бессрочно)
    /// </summary>
    public DateTime? ExpiredAt { get; private set; }

    protected Restriction(UserId userId, PolicyType type, UserId createdBy, DateTime createdAt, DateTime? expiredAt)
    {
        UserId = userId;
        Type = type;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        ExpiredAt = expiredAt;
    }
}

public sealed class PortalRestriction : Restriction
{
    public PortalRestriction(UserId userId, PolicyType type, UserId createdBy, DateTime createdAt, DateTime? expiredAt)
        : base(userId, type, createdBy, createdAt, expiredAt)
    {
    }
}

[Include(typeof(Forum), PropertyGenerationMode.AsPrivateSet, nameof(Forum.ForumId))]
public sealed partial class ForumRestriction : Restriction
{
    public ForumRestriction(ForumId forumId, UserId userId, PolicyType type, UserId createdBy, DateTime createdAt,
        DateTime? expiredAt)
        : base(userId, type, createdBy, createdAt, expiredAt)
    {
        ForumId = forumId;
    }
}

[Include(typeof(Category), PropertyGenerationMode.AsPrivateSet, nameof(Category.CategoryId))]
public sealed partial class CategoryRestriction : Restriction
{
    public CategoryRestriction(CategoryId categoryId, UserId userId, PolicyType type, UserId createdBy,
        DateTime createdAt, DateTime? expiredAt)
        : base(userId, type, createdBy, createdAt, expiredAt)
    {
        CategoryId = categoryId;
    }
}

[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadRestriction : Restriction
{
    public ThreadRestriction(ThreadId threadId, UserId userId, PolicyType type, UserId createdBy, DateTime createdAt,
        DateTime? expiredAt)
        : base(userId, type, createdBy, createdAt, expiredAt)
    {
        ThreadId = threadId;
    }
}