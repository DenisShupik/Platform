using CoreService.Domain.Enums;

namespace CoreService.Application.Dtos;

public sealed record CapabilityDefinitionDto
{
    public required CapabilityCode Capability { get; init; }
    public required IReadOnlyList<AuthorizationScopeType> AllowedScopes { get; init; }
}
