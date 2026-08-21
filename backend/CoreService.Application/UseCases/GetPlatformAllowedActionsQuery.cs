using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetPlatformAllowedActionsQuery : IQuery<PlatformAllowedActionsDto>
{
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetPlatformAllowedActionsQueryHandler(IForumPolicyEvaluator policies) :
    IQueryHandler<GetPlatformAllowedActionsQuery, PlatformAllowedActionsDto>
{
    public async Task<PlatformAllowedActionsDto> HandleAsync(
        GetPlatformAllowedActionsQuery query,
        CancellationToken cancellationToken)
    {
        var allowed = await policies.GetAllowedAsync(
            query.RequestedBy,
            AuthorizationScope.Platform,
            query.EvaluatedAt,
            cancellationToken);

        return new PlatformAllowedActionsDto
        {
            CanManageStructure = allowed.Contains(ForumPolicy.ManageStructure),
            CanManageAuthorization = allowed.Contains(ForumPolicy.ManageAuthorization),
            CanManageSanctions = allowed.Contains(ForumPolicy.ManageSanctions)
        };
    }
}
