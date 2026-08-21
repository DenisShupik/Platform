using CoreService.Domain.ValueObjects;
using ProtoBuf;
using Shared.Domain.Abstractions;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetThreadsRequest
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    [ProtoMember(1)]
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }

    /// <summary>
    /// Пользователь, от имени которого сервис запрашивает темы
    /// </summary>
    [ProtoMember(2)]
    public RequestedByActor? RequestedBy { get; init; }
}

[ProtoContract]
public sealed class GetThreadsResponse
{
    /// <summary>
    /// Темы
    /// </summary>
    [ProtoMember(1)]
    public required List<GetThreadResponse> Threads { get; init; }
}
