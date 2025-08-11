using ProtoBuf;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetThreadRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required Guid ThreadId { get; init; }
}

[ProtoContract]
public sealed class GetThreadResponse
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required Guid ThreadId { get; init; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [ProtoMember(2)]
    public required Guid CategoryId { get; init; }

    /// <summary>
    /// Название темы
    /// </summary>
    [ProtoMember(3)]
    public required string Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    [ProtoMember(4)]
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    [ProtoMember(5)]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Последний использованный идентификатор сообщения
    /// </summary>
    [ProtoMember(6)]
    public required long NextPostId { get; init; }
}