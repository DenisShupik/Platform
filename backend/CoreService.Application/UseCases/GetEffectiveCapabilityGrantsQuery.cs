using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetEffectiveCapabilityGrantsQuery : IQuery<Result<
    IReadOnlyList<CapabilityGrantDto>,
    PermissionDeniedError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError>>
{
    public required UserId UserId { get; init; }
    public required AuthorizationScopeType ScopeType { get; init; }
    public ForumId? ForumId { get; init; }
    public CategoryId? CategoryId { get; init; }
    public ThreadId? ThreadId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetEffectiveCapabilityGrantsQueryHandler(
    IAuthorizationScopeResolver scopes,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies) : IQueryHandler<GetEffectiveCapabilityGrantsQuery, Result<
    IReadOnlyList<CapabilityGrantDto>,
    PermissionDeniedError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError>>
{
    public async Task<Result<IReadOnlyList<CapabilityGrantDto>, PermissionDeniedError,
        AuthorizationScopeNotFoundError, InvalidAuthorizationScopeError>> HandleAsync(
        GetEffectiveCapabilityGrantsQuery query,
        CancellationToken cancellationToken)
    {
        var scopeResult = await scopes.ResolveAsync(
            query.ScopeType,
            query.ForumId,
            query.CategoryId,
            query.ThreadId,
            cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var scopeError)) return scopeError;

        var authorization = await policies.AuthorizeAsync(
            query.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            query.EvaluatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        return (await grants.GetEffectiveGrantsAsync(
            query.UserId,
            scope,
            query.EvaluatedAt,
            cancellationToken)).ToList();
    }
}
