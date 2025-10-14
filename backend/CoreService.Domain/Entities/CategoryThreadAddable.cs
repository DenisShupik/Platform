using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class CategoryThreadAddable : IHasCategoryId
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public PolicyId ReadPolicyId { get; private set; }

    /// <summary>
    /// Политика доступа
    /// </summary>
    public Policy ReadPolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public PolicyId PostCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания сообщения
    /// </summary>
    public Policy PostCreatePolicy { get; private set; }

    public ICollection<Thread> Threads { get; private set; } = [];

    public Result<Thread, PolicyDowngradeError> AddThread(ThreadTitle title, UserId? createdBy, DateTime createdAt,
        PolicyValue? readPolicyValue, PolicyValue? postCreatePolicyValue)
    {
        PolicyId readPolicyId;
        {
            if (!ReadPolicy.TryGetOrCreate(readPolicyValue).TryGet(out readPolicyId, out var error)) return error;
        }

        PolicyId postCreatePolicyId;
        {
            if (!PostCreatePolicy.TryGetOrCreate(postCreatePolicyValue)
                    .TryGet(out postCreatePolicyId, out var error)) return error;
        }

        var thread = new Thread(CategoryId, title, createdBy, createdAt, readPolicyId, postCreatePolicyId);
        Threads.Add(thread);
        return thread;
    }
}