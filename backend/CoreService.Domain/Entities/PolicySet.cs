using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Политики
/// </summary>
public abstract class Policies : IHasUpdateProperties
{
    /// <summary>
    /// Политика доступа
    /// </summary>
    public Policy Access { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего набор политик
    /// </summary>
    public UserId UpdatedBy { get; private set; }

    /// <summary>
    /// Дата и время последнего изменения набора политик
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    protected Policies(Policy access, UserId updatedBy, DateTime updatedAt)
    {
        Access = access;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }
}

public abstract class ThreadPolicies : Policies
{
    /// <summary>
    /// Политика создания сообщения
    /// </summary>
    public Policy PostCreate { get; private set; }

    protected ThreadPolicies(Policy postCreate, Policy access, UserId updatedBy, DateTime updatedAt) : base(access,
        updatedBy, updatedAt)
    {
        PostCreate = postCreate;
    }
}

public sealed class ThreadPolicySet : ThreadPolicies
{
    /// <summary>
    /// Идентификатор набора политик темы
    /// </summary>
    public ThreadPolicySetId ThreadPolicySetId { get; private set; }

    public ThreadPolicySet(Policy postCreate, Policy access, UserId updatedBy, DateTime updatedAt) : base(postCreate,
        access, updatedBy, updatedAt)
    {
        ThreadPolicySetId = ThreadPolicySetId.From(Guid.CreateVersion7());
    }
}

public abstract class CategoryPolicies : ThreadPolicies
{
    /// <summary>
    /// Политика создания темы
    /// </summary>
    public Policy ThreadCreate { get; private set; }

    protected CategoryPolicies(Policy threadCreate, Policy postCreate, Policy access, UserId updatedBy,
        DateTime updatedAt) : base(postCreate, access, updatedBy, updatedAt)
    {
        ThreadCreate = threadCreate;
    }
}

public sealed class CategoryPolicySet : CategoryPolicies
{
    /// <summary>
    /// Идентификатор набора политик раздела
    /// </summary>
    public CategoryPolicySetId CategoryPolicySetId { get; private set; }

    public CategoryPolicySet(Policy threadCreate, Policy postCreate, Policy access, UserId updatedBy,
        DateTime updatedAt) : base(threadCreate, postCreate, access, updatedBy, updatedAt)
    {
        CategoryPolicySetId = CategoryPolicySetId.From(Guid.CreateVersion7());
    }
}

public abstract class ForumPolicies : CategoryPolicies
{
    /// <summary>
    /// Политика создания раздела
    /// </summary>
    public Policy CategoryCreate { get; private set; }

    protected ForumPolicies(Policy categoryCreate, Policy threadCreate, Policy postCreate, Policy access,
        UserId updatedBy, DateTime updatedAt) : base(threadCreate, postCreate, access, updatedBy, updatedAt)
    {
        CategoryCreate = categoryCreate;
    }
}

public sealed class ForumPolicySet : ForumPolicies
{
    /// <summary>
    /// Идентификатор набора политик форума
    /// </summary>
    public ForumPolicySetId ForumPolicySetId { get; private set; }

    public ForumPolicySet(Policy categoryCreate, Policy threadCreate, Policy postCreate, Policy access,
        UserId updatedBy, DateTime updatedAt) : base(categoryCreate, threadCreate,
        postCreate, access, updatedBy, updatedAt)
    {
        ForumPolicySetId = ForumPolicySetId.From(Guid.CreateVersion7());
    }
}