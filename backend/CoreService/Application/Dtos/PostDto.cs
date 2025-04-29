using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class PostDto : IHasThreadId, IHasCreateProperties
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
    public PostContent Content { get; set; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    public UserId CreatedBy { get; set; }
    
    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; set; }
}