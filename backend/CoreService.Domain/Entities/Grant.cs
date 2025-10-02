using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Разрешение
/// </summary>
[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
public abstract partial class Grant : IHasCreateProperties
{
    /// <summary>
    /// Политика
    /// </summary>
    public PolicyType Policy { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего разрешение
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания разрешения
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    protected Grant(UserId userId, PolicyType policy, UserId createdBy, DateTime createdAt)
    {
        UserId = userId;
        Policy = policy;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}

[Include(typeof(Forum), PropertyGenerationMode.AsPrivateSet, nameof(Forum.ForumId))]
public sealed partial class ForumGrant : Grant
{
    public ForumGrant(ForumId forumId, UserId userId, PolicyType policy, UserId createdBy, DateTime createdAt)
        : base(userId, policy, createdBy, createdAt)
    {
        ForumId = forumId;
    }
}

[Include(typeof(Category), PropertyGenerationMode.AsPrivateSet, nameof(Category.CategoryId))]
public sealed partial class CategoryGrant : Grant
{
    public CategoryGrant(CategoryId categoryId, UserId userId, PolicyType policy, UserId createdBy, DateTime createdAt)
        : base(userId, policy, createdBy, createdAt)
    {
        CategoryId = categoryId;
    }
}

[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadGrant : Grant
{
    public ThreadGrant(ThreadId threadId, UserId userId, PolicyType policy, UserId createdBy, DateTime createdAt)
        : base(userId, policy, createdBy, createdAt)
    {
        ThreadId = threadId;
    }
}