using TopicService.Domain.Abstractions;

namespace TopicService.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public sealed class Section : IHasCreatedProperties
{
    public const int TitleMaxLength = 256;

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public int SectionId { get; set; }

    /// <summary>
    /// Наименование раздела
    /// </summary>
    public int Title { get; set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Категории раздела
    /// </summary>
    public ICollection<Category> Categories { get; set; }
}