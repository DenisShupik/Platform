using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class ForumCategoryAddable : IHasForumId
{
    public ForumId ForumId { get; }
    public ICollection<Category> Categories { get; private set; } = [];

    public Category AddCategory(CategoryTitle title, UserId createdBy, DateTime createdAt)
    {
        var category = new Category(ForumId, title, createdBy, createdAt);
        Categories.Add(category);
        return category;
    }
}