using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumModeratorsQuery : IQuery<
    Result<IReadOnlyList<ForumModeratorAppointmentDto>, PermissionDeniedError, ForumNotFoundError>>
{
    public required ForumId ForumId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetForumModeratorsQueryHandler(
    IForumReadRepository forums,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetForumModeratorsQuery,
    Result<IReadOnlyList<ForumModeratorAppointmentDto>, PermissionDeniedError, ForumNotFoundError>>
{
    public async Task<Result<IReadOnlyList<ForumModeratorAppointmentDto>, PermissionDeniedError, ForumNotFoundError>>
        HandleAsync(GetForumModeratorsQuery query, CancellationToken cancellationToken)
    {
        var scopeResult = await forums.GetAuthorizationScopeAsync(query.ForumId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var forumError)) return forumError;

        var authorization = await policies.AuthorizeAsync(
            query.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            query.EvaluatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var appointments = await grants.GetActiveForumModeratorAppointmentsAsync(
            query.ForumId,
            query.EvaluatedAt,
            cancellationToken);

        return appointments.ToList();
    }
}
