using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class ForumCategoryAddable : IHasForumId
{
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