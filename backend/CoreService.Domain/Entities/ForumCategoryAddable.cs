using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class ForumCategoryAddable : IHasForumId
{
    public ICollection<Category> Categories { get; private set; } = [];

    public Category AddCategory(CategoryTitle title, UserId createdBy, DateTime createdAt, CategoryPolicySetId? categoryPolicySetId)
    {
        var category = new Category(ForumId, title, createdBy, createdAt, categoryPolicySetId);
        Categories.Add(category);
        return category;
    }
}