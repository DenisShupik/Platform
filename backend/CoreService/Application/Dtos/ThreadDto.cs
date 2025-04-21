using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class ThreadDto : IHasCategoryId, IHasThreadId, IHasCreateProperties
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; set; }

    /// <summary>
    /// Последний использованный идентификатор сообщения
    /// </summary>
    public long PostIdSeq { get; set; }

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
}