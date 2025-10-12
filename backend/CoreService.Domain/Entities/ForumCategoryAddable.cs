using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class ForumCategoryAddable : IHasForumId
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public PolicyId AccessPolicyId { get; private set; }

    /// <summary>
    /// Политика доступа
    /// </summary>
    public Policy AccessPolicy { get; private set; }

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

    public Category AddCategory(CategoryTitle title, UserId? createdBy, DateTime createdAt, PolicyId accessPolicyId,
        PolicyId threadCreatePolicyId, PolicyId postCreatePolicyId)
    {
        var category = new Category(ForumId, title, createdBy, createdAt, accessPolicyId, threadCreatePolicyId,
            postCreatePolicyId);
        Categories.Add(category);
        return category;
    }
}