using ProtoBuf;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class RequestedByActor
{
    [ProtoMember(1)]
    public required UserId UserId { get; init; }
}
