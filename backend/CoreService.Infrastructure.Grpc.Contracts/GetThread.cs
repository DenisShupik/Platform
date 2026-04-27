using CoreService.Domain.ValueObjects;
using ProtoBuf;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetThreadRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required ThreadId ThreadId { get; init; }
}

[ProtoContract]
public sealed class GetThreadResponse
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [ProtoMember(2)]
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Название темы
    /// </summary>
    [ProtoMember(3)]
    public required ThreadTitle Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    [ProtoMember(4)]
    public required UserId? CreatedBy { get; init; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    [ProtoMember(5)]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    [ProtoMember(6)]
    public required ThreadState State { get; init; }

    /// <summary>
    /// Количество сообщений в теме
    /// </summary>
    [ProtoMember(7)]
    public required Count PostCount { get; init; }
}