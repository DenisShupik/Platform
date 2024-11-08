using CoreService.Domain.Abstractions;

namespace CoreService.Domain.Entities;

/// <summary>
/// Тема
/// </summary>
public sealed class Thread : IHasCreatedProperties
{
    public const int TitleMaxLength = 256;

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public long ThreadId { get; set; }

    /// <summary>
    /// Последний использованный идентификтаор сообщения
    /// </summary>
    public long PostIdSeq { get; set; }

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public long CategoryId { get; set; }

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
    /// Сообщения темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }
}