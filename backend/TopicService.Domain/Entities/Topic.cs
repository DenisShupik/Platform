namespace TopicService.Domain.Entities;

/// <summary>
/// Тема
/// </summary>
public sealed class Topic
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public long TopicId { get; set; }
    
    /// <summary>
    /// Название темы
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Список постов темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }
}