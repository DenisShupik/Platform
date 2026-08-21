using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumAllowedActionsQuery : IQuery<Result<ForumAllowedActionsDto, ForumNotFoundError>>
{
    public required ForumId ForumId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetForumAllowedActionsQueryHandler(
    IForumReadRepository forums,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetForumAllowedActionsQuery,
    Result<ForumAllowedActionsDto, ForumNotFoundError>>
{
    public async Task<Result<ForumAllowedActionsDto, ForumNotFoundError>> HandleAsync(
        GetForumAllowedActionsQuery query,
        CancellationToken cancellationToken)
    {
        var scopeResult = await forums.GetAuthorizationScopeAsync(query.ForumId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var forumError)) return forumError;

        var allowed = await policies.GetAllowedAsync(
            query.RequestedBy,
            scope,
            query.EvaluatedAt,
            cancellationToken);

        return new ForumAllowedActionsDto
        {
            CanManageStructure = allowed.Contains(ForumPolicy.ManageStructure),
            CanManageAuthorization = allowed.Contains(ForumPolicy.ManageAuthorization),
            CanManageSanctions = allowed.Contains(ForumPolicy.ManageSanctions)
        };
    }
}
