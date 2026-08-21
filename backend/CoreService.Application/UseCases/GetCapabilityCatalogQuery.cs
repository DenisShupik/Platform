using CoreService.Application.Dtos;
using CoreService.Domain.Authorization;
using CoreService.Domain.Enums;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

public sealed class GetCapabilityCatalogQuery : IQuery<IReadOnlyList<CapabilityDefinitionDto>>;

public sealed class GetCapabilityCatalogQueryHandler : IQueryHandler<
    GetCapabilityCatalogQuery,
    IReadOnlyList<CapabilityDefinitionDto>>
{
    public Task<IReadOnlyList<CapabilityDefinitionDto>> HandleAsync(
        GetCapabilityCatalogQuery query,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CapabilityDefinitionDto>>(
            Enum.GetValues<CapabilityCode>()
                .Select(capability => new CapabilityDefinitionDto
                {
                    Capability = capability,
                    AllowedScopes = CapabilityScopePolicy.GetAllowedScopes(capability)
                })
                .ToArray());
}
