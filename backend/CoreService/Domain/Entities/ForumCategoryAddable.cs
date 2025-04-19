using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class ForumCategoryAddable : IHasForumId
{
    public ForumId ForumId { get; }
    public ICollection<Category> Categories { get; private set; } = [];

    public void AddCategory(Category category)
    {
        Categories.Add(category);
    }
}