using CoreService.Application.Interfaces;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class PostDto : IHasThreadId, IHasCreatedProperties
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public PostId PostId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; set; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    public UserId CreatedBy { get; set; }
}