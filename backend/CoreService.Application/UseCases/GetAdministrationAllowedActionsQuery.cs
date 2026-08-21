using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetAdministrationAllowedActionsQuery : IQuery<AdministrationAllowedActionsDto>
{
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetAdministrationAllowedActionsQueryHandler(
    ICapabilityGrantRepository grants) : IQueryHandler<
    GetAdministrationAllowedActionsQuery,
    AdministrationAllowedActionsDto>
{
    public async Task<AdministrationAllowedActionsDto> HandleAsync(
        GetAdministrationAllowedActionsQuery query,
        CancellationToken cancellationToken)
    {
        var capabilities = await grants.GetActiveCapabilityScopesAsync(
            query.RequestedBy.UserId,
            new HashSet<CapabilityCode>
            {
                CapabilityCode.ManageAuthorization,
                CapabilityCode.ManageSanctions
            },
            query.EvaluatedAt,
            cancellationToken);

        return new AdministrationAllowedActionsDto
        {
            CanManageAnyAuthorization = capabilities.Any(entry =>
                entry.Capability == CapabilityCode.ManageAuthorization),
            CanManageAnySanctions = capabilities.Any(entry =>
                entry.Capability == CapabilityCode.ManageSanctions),
            CanManagePlatformAuthorization = capabilities.Contains(
                (CapabilityCode.ManageAuthorization, AuthorizationScopeType.Platform)),
            CanManagePlatformSanctions = capabilities.Contains(
                (CapabilityCode.ManageSanctions, AuthorizationScopeType.Platform))
        };
    }
}
