using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class ThreadDto : IHasCreatedProperties
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
}