using ProtoBuf;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetThreadsRequest
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    [ProtoMember(1)]
    public required HashSet<Guid> ThreadIds { get; init; }
}

[ProtoContract]
public sealed class GetThreadsResponse
{
    /// <summary>
    /// Темы
    /// </summary>
    [ProtoMember(1)]
    public required GetThreadResponse[] Threads { get; init; }
}