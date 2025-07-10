using ProtoBuf;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetPostRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public Guid ThreadId { get; set; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    [ProtoMember(2)]
    public long PostId { get; set; }
}

[ProtoContract]
public sealed class GetPostResponse
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public Guid ThreadId { get; set; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    [ProtoMember(2)]
    public long PostId { get; set; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    [ProtoMember(3)]
    public string Content { get; set; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    [ProtoMember(4)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    [ProtoMember(5)]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Дата и время последнего изменения сообщения
    /// </summary>
    [ProtoMember(6)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего сообщение
    /// </summary>
    [ProtoMember(7)]
    public Guid UpdatedBy { get; set; }
}