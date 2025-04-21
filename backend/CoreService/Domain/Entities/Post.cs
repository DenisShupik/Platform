using System.ComponentModel.DataAnnotations;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Сообщение
/// </summary>
public sealed class Post : IHasCreateProperties, IHasUpdateProperties
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
    /// Дата и время последнего изменения сообщения
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего сообщение
    /// </summary>
    public UserId UpdatedBy { get; set; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public uint RowVersion { get; set; }
}