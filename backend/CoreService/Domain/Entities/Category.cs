using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public sealed class Category : IHasCreatedProperties
{ 
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; set; }
    
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public CategoryTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public UserId CreatedBy { get; set; }
    
    /// <summary>
    /// Темы раздела
    /// </summary>
    public ICollection<Thread> Threads { get; set; }
}