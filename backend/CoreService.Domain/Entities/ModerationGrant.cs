using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
public abstract partial class ModerationGrant : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор пользователя, создавшего грант
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время создания гранта
    /// </summary>
    public UserId CreatedBy { get; private set; }

    protected ModerationGrant(UserId userId, UserId createdBy, DateTime createdAt)
    {
        UserId = userId;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}

[Include(typeof(Forum), PropertyGenerationMode.AsPrivateSet, nameof(Forum.ForumId))]
public sealed partial class ForumModerationGrant : ModerationGrant
{
    public ForumModerationGrant(ForumId forumId, UserId userId, UserId createdBy, DateTime createdAt) : base(userId,
        createdBy, createdAt)
    {
        ForumId = forumId;
    }
}

[Include(typeof(Category), PropertyGenerationMode.AsPrivateSet, nameof(Category.CategoryId))]
public sealed partial class CategoryModerationGrant : ModerationGrant
{
    public CategoryModerationGrant(CategoryId categoryId, UserId userId, UserId createdBy, DateTime createdAt) : base(
        userId,
        createdBy, createdAt)
    {
        CategoryId = categoryId;
    }
}

[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadModerationGrant : ModerationGrant
{
    public ThreadModerationGrant(ThreadId threadId, UserId userId, UserId createdBy, DateTime createdAt) : base(userId,
        createdBy, createdAt)
    {
        ThreadId = threadId;
    }
}