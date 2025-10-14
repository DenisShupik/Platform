using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class Portal
{
    public short PortalId { get; private set; }

    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public PolicyId ReadPolicyId { get; private set; }

    /// <summary>
    /// Политика доступа
    /// </summary>
    public ReadPolicy ReadPolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания форума
    /// </summary>
    public PolicyId ForumCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания форума
    /// </summary>
    public ForumCreatePolicy ForumCreatePolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания раздела
    /// </summary>
    public PolicyId CategoryCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания раздела
    /// </summary>
    public CategoryCreatePolicy CategoryCreatePolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания темы
    /// </summary>
    public PolicyId ThreadCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания темы
    /// </summary>
    public ThreadCreatePolicy ThreadCreatePolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public PolicyId PostCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания сообщения
    /// </summary>
    public PostCreatePolicy PostCreatePolicy { get; private set; }

    public Result<Forum, PolicyDowngradeError> AddForum(ForumTitle title, UserId? createdBy, DateTime createdAt,
        PolicyValue? readPolicyValue, PolicyValue? categoryCreatePolicyValue, PolicyValue? threadCreatePolicyValue,
        PolicyValue? postCreatePolicyValue)
    {
        PolicyId readPolicyId;
        {
            if (!ReadPolicy.TryGetOrCreate(readPolicyValue).TryGet(out readPolicyId, out var error)) return error;
        }

        PolicyId categoryCreatePolicyId;
        {
            if (!CategoryCreatePolicy.TryGetOrCreate(categoryCreatePolicyValue)
                    .TryGet(out categoryCreatePolicyId, out var error)) return error;
        }

        PolicyId threadCreatePolicyId;
        {
            if (!ThreadCreatePolicy.TryGetOrCreate(threadCreatePolicyValue)
                    .TryGet(out threadCreatePolicyId, out var error)) return error;
        }

        PolicyId postCreatePolicyId;
        {
            if (!PostCreatePolicy.TryGetOrCreate(postCreatePolicyValue)
                    .TryGet(out postCreatePolicyId, out var error)) return error;
        }

        var forum = new Forum(title, createdBy, createdAt, readPolicyId, categoryCreatePolicyId, threadCreatePolicyId,
            postCreatePolicyId);

        return forum;
    }
}