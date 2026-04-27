using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public sealed class Category : IHasCategoryId
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; private set; }

    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; private set; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public CategoryTitle Title { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    internal Category(ForumId forumId, CategoryTitle title, UserId createdBy, DateTime createdAt)
    {
        CategoryId = CategoryId.From(Guid.CreateVersion7());
        ForumId = forumId;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public Thread AddThread(ThreadTitle title, UserId createdBy, DateTime createdAt)
    {
        return new Thread(CategoryId, title, createdBy, createdAt);
    }
}