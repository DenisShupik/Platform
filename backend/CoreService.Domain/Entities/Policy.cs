using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Domain.Entities;

/// <summary>
/// Политика
/// </summary>
public abstract class Policy
{
    /// <summary>
    /// Идентификатор политики
    /// </summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>
    /// Тип политики
    /// </summary>
    public PolicyType Type { get; private set; }

    /// <summary>
    /// Значение политики
    /// </summary>
    public PolicyValue Value { get; private set; }

    public PolicyId? ParentId { get; private set; }

    public ICollection<Policy>? AddedPolicies { get; private set; }

    private protected Policy(PolicyType type, PolicyValue value, PolicyId? parentId)
    {
        PolicyId = PolicyId.From(Guid.CreateVersion7());
        Type = type;
        Value = value;
        ParentId = parentId;
    }

    private protected abstract Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId);

    public Result<PolicyId, PolicyDowngradeError> TryGetOrCreate(PolicyValue? value)
    {
        if (value == null) return PolicyId;

        if (value == PolicyValue.Granted || Value < value)
        {
            var policy = CreateInstance(value.Value, PolicyId);
            // if (value == PolicyValue.Granted)
            // {
            //    
            //    
            // }
            AddedPolicies ??= new List<Policy>();
            AddedPolicies.Add(policy);
            return policy.PolicyId;
        }

        return new PolicyDowngradeError();
    }
}

public sealed class ReadPolicy : Policy
{
    private ReadPolicy(PolicyValue value, PolicyId? parentId) : base(PolicyType.Read, value, parentId)
    {
    }

    private protected override Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId) =>
        new ReadPolicy(value, parentPolicyId);
}

public sealed class ForumCreatePolicy : Policy
{
    private ForumCreatePolicy(PolicyValue value, PolicyId? parentId) : base(PolicyType.ForumCreate, value,
        parentId)
    {
    }

    private protected override Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId) =>
        new ForumCreatePolicy(value, parentPolicyId);
}

public sealed class CategoryCreatePolicy : Policy
{
    private CategoryCreatePolicy(PolicyValue value, PolicyId? parentId) : base(PolicyType.CategoryCreate, value,
        parentId)
    {
    }

    private protected override Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId) =>
        new CategoryCreatePolicy(value, parentPolicyId);
}

public sealed class ThreadCreatePolicy : Policy
{
    private ThreadCreatePolicy(PolicyValue value, PolicyId? parentId) : base(PolicyType.ThreadCreate, value,
        parentId)
    {
    }

    private protected override Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId) =>
        new ThreadCreatePolicy(value, parentPolicyId);
}

public sealed class PostCreatePolicy : Policy
{
    private PostCreatePolicy(PolicyValue value, PolicyId? parentId) : base(PolicyType.PostCreate, value,
        parentId)
    {
    }

    private protected override Policy CreateInstance(PolicyValue value, PolicyId? parentPolicyId) =>
        new PostCreatePolicy(value, parentPolicyId);
}