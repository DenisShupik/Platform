using CoreService.Domain.Abstractions;

namespace CoreService.Domain.Entities;

/// <summary>
/// Категория
/// </summary>
public sealed class Category : IHasCreatedProperties
{
    public const int TitleMaxLength = 256;

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public long CategoryId { get; set; }
    
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public long ForumId { get; set; }

    /// <summary>
    /// Наименование категории
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Дата и время создания категории
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего категорию
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Темы категории
    /// </summary>
    public ICollection<Thread> Threads { get; set; }
}