using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Тема
/// </summary>
public sealed class Thread : IHasCreateProperties
{
    public const int TitleMaxLength = 256;

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; set; }

    /// <summary>
    /// Последний использованный идентификатор сообщения
    /// </summary>
    public PostId NextPostId { get; set; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public ThreadTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    public UserId CreatedBy { get; set; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public ThreadStatus Status { get; set; }
    
    /// <summary>
    /// Сообщения темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }
}