using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public sealed class Forum : IHasCreatedProperties
{ 
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Наименование раздела
    /// </summary>
    public ForumTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public UserId CreatedBy { get; set; }

    /// <summary>
    /// Категории раздела
    /// </summary>
    public ICollection<Category> Categories { get; set; }
}