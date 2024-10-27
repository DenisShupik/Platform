namespace TopicService.Domain.Entities;

/// <summary>
/// Сообщение
/// </summary>
public sealed class Post
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public long PostId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public long TopicId { get; set; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public string Сontent { get; set; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    public Guid CreatedBy { get; set; }
}