using ProtoBuf;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetThreadRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public Guid ThreadId { get; set; }
}

[ProtoContract]
public sealed class GetThreadResponse
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public Guid ThreadId { get; set; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [ProtoMember(2)]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Название темы
    /// </summary>
    [ProtoMember(3)]
    public string Title { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    [ProtoMember(4)]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    [ProtoMember(5)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Последний использованный идентификатор сообщения
    /// </summary>
    [ProtoMember(6)]
    public long NextPostId { get; set; }
}