using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class ForumCategoryAddable : IHasForumId
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
    /// Идентификатор политики создания темы
    /// </summary>
    public PolicyId ThreadCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания темы
    /// </summary>
    public Policy ThreadCreatePolicy { get; private set; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public PolicyId PostCreatePolicyId { get; private set; }

    /// <summary>
    /// Политика создания сообщения
    /// </summary>
    public Policy PostCreatePolicy { get; private set; }

    public ICollection<Category> Categories { get; private set; } = [];

    public Result<Category, PolicyDowngradeError> AddCategory(CategoryTitle title, UserId? createdBy, DateTime createdAt, PolicyValue? readPolicyValue, PolicyValue? threadCreatePolicyValue,
        PolicyValue? postCreatePolicyValue)
    {
        PolicyId readPolicyId;
        {
            if (!ReadPolicy.TryGetOrCreate(readPolicyValue).TryGet(out readPolicyId, out var error)) return error;
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
        
        var category = new Category(ForumId, title, createdBy, createdAt, readPolicyId, threadCreatePolicyId,
            postCreatePolicyId);
        Categories.Add(category);
        return category;
    }
}