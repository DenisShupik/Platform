using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetPlatformAdministratorsQuery : IQuery<
    Result<IReadOnlyList<PlatformAdministratorAppointmentDto>, PermissionDeniedError>>
{
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetPlatformAdministratorsQueryHandler(
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetPlatformAdministratorsQuery,
    Result<IReadOnlyList<PlatformAdministratorAppointmentDto>, PermissionDeniedError>>
{
    public async Task<Result<IReadOnlyList<PlatformAdministratorAppointmentDto>, PermissionDeniedError>> HandleAsync(
        GetPlatformAdministratorsQuery query,
        CancellationToken cancellationToken)
    {
        var authorization = await policies.AuthorizeAsync(
            query.RequestedBy,
            ForumPolicy.ManageAuthorization,
            AuthorizationScope.Platform,
            query.EvaluatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        return (await grants.GetActivePlatformAdministratorAppointmentsAsync(
            query.EvaluatedAt,
            cancellationToken)).ToList();
    }
}
