using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Форум
/// </summary>
public sealed class Forum : IHasCreateProperties
{
    private Forum()
    {
    }

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
    /// Уровень доступа
    /// </summary>
    public AccessLevel AccessLevel { get; private set; }
    
    public ForumPolicies Policies { get; private set; }

    /// <summary>
    /// Разделы форума
    /// </summary>
    public ICollection<Category> Categories { get; set; }

    public Forum(ForumTitle title, UserId createdBy, DateTime createdAt, AccessLevel accessLevel,
        ForumPolicies policies)
    {
        ForumId = ForumId.From(Guid.CreateVersion7());
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        AccessLevel = accessLevel;
        Policies = policies;
    }
}

public sealed class ForumPolicies
{
    public CategoryCreatePolicy CategoryCreate { get; private set; }

    public ForumPolicies(CategoryCreatePolicy categoryCreate)
    {
        CategoryCreate = categoryCreate;
    }
}