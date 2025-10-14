using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using ProtoBuf;
using UserService.Domain.ValueObjects;

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
    public required ThreadStatus Status { get; init; }

    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    [ProtoMember(7)]
    public required PolicyId ReadPolicyId { get; init; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    [ProtoMember(8)]
    public required PolicyId PostCreatePolicyId { get; init; }
}