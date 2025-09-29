using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
public abstract partial class AccessGrant : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор пользователя, создавшего грант.
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания гранта
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    protected AccessGrant(UserId userId, UserId createdBy, DateTime createdAt)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}

[Include(typeof(Forum), PropertyGenerationMode.AsPrivateSet, nameof(Forum.ForumId))]
public sealed partial class ForumAccessGrant : AccessGrant
{
    public ForumAccessGrant(ForumId forumId, UserId userId, UserId createdBy, DateTime createdAt) : base(userId,
        createdBy, createdAt)
    {
        ForumId = forumId;
    }
}

[Include(typeof(Category), PropertyGenerationMode.AsPrivateSet, nameof(Category.CategoryId))]
public sealed partial class CategoryAccessGrant : AccessGrant
{
    public CategoryAccessGrant(CategoryId categoryId, UserId userId, UserId createdBy, DateTime createdAt) : base(
        userId,
        createdBy, createdAt)
    {
        CategoryId = categoryId;
    }
}

[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadAccessGrant : AccessGrant
{
    public ThreadAccessGrant(ThreadId threadId, UserId userId, UserId createdBy, DateTime createdAt) : base(userId,
        createdBy, createdAt)
    {
        ThreadId = threadId;
    }
}