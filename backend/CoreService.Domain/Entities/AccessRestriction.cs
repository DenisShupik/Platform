using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
public abstract partial class AccessRestriction
{
    public RestrictionLevel RestrictionLevel { get; private set; }

    protected AccessRestriction(UserId userId, RestrictionLevel restrictionLevel)
    {
    }
}

[Include(typeof(Forum), PropertyGenerationMode.AsPrivateSet, nameof(Forum.ForumId))]
public sealed partial class ForumAccessRestriction : AccessRestriction
{
    public ForumAccessRestriction(ForumId forumId, UserId userId, RestrictionLevel restrictionLevel) : base(userId,
        restrictionLevel)
    {
        ForumId = forumId;
    }
}

[Include(typeof(Category), PropertyGenerationMode.AsPrivateSet, nameof(Category.CategoryId))]
public sealed partial class CategoryAccessRestriction : AccessRestriction
{
    public CategoryAccessRestriction(CategoryId categoryId, UserId userId, RestrictionLevel restrictionLevel) : base(
        userId, restrictionLevel)
    {
        CategoryId = categoryId;
    }
}

[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadAccessRestriction : AccessRestriction
{
    public ThreadAccessRestriction(ThreadId threadId, UserId userId, RestrictionLevel restrictionLevel) : base(userId, restrictionLevel)
    {
        ThreadId = threadId;
    }
}