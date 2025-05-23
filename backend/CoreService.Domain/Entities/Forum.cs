using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Форум
/// </summary>
public sealed class Forum : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; private set; }

    /// <summary>
    /// Название форума
    /// </summary>
    public ForumTitle Title { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания форума
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Разделы форума
    /// </summary>
    public ICollection<Category> Categories { get; set; }
    
    public Forum(ForumTitle title, UserId createdBy, DateTime createdAt)
    {
        ForumId = ForumId.From(Guid.CreateVersion7());
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}