using CoreService.Domain.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class PostDto : IHasCreatedProperties
{ 
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public long PostId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public long ThreadId { get; set; }

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