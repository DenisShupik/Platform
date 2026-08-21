using ProtoBuf;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GrantInitialPlatformAdministratorCapabilitiesRequest
{
    [ProtoMember(1)]
    public required UserId UserId { get; init; }
}

[ProtoContract]
public sealed class GrantInitialPlatformAdministratorCapabilitiesResponse;
