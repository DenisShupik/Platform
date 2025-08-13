using CoreService.Domain.ValueObjects;
using ProtoBuf;
using UserService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetPostRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    [ProtoMember(2)]
    public required PostId PostId { get; init; }
}

[ProtoContract]
public sealed class GetPostResponse
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [ProtoMember(1)]
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    [ProtoMember(2)]
    public required PostId PostId { get; init; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    [ProtoMember(3)]
    public required PostContent Content { get; init; }

    /// <summary>
    /// Дата и время создания сообщения
    /// </summary>
    [ProtoMember(4)]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего сообщение
    /// </summary>
    [ProtoMember(5)]
    public required UserId CreatedBy { get; init; }

    /// <summary>
    /// Дата и время последнего изменения сообщения
    /// </summary>
    [ProtoMember(6)]
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Идентификатор пользователя, последним изменившего сообщение
    /// </summary>
    [ProtoMember(7)]
    public required UserId UpdatedBy { get; init; }
}