using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

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
    public ThreadId ThreadId { get; set; }

    /// <summary>
    /// Последний использованный идентификатор сообщения
    /// </summary>
    public long PostIdSeq { get; set; }

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public CategoryId CategoryId { get; set; }

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
    public UserId CreatedBy { get; set; }

    /// <summary>
    /// Сообщения темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }
}