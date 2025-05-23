using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public sealed class Category : IHasCreateProperties
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

    /// <summary>
    /// Темы раздела
    /// </summary>
    public ICollection<Thread> Threads { get; set; }

    internal Category(ForumId forumId, CategoryTitle title, UserId createdBy, DateTime createdAt)
    {
        CategoryId = CategoryId.From(Guid.CreateVersion7());
        ForumId = forumId;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}