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

public sealed class GetForumSanctionsQuery : IQuery<Result<
    IReadOnlyList<ForumSanctionDto>,
    PermissionDeniedError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError>>
{
    public required AuthorizationScopeType ScopeType { get; init; }
    public ForumId? ForumId { get; init; }
    public CategoryId? CategoryId { get; init; }
    public ThreadId? ThreadId { get; init; }
    public required bool IncludeInactive { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetForumSanctionsQueryHandler(
    IAuthorizationScopeResolver scopes,
    IForumSanctionRepository sanctions,
    IForumPolicyEvaluator policies) : IQueryHandler<GetForumSanctionsQuery, Result<
    IReadOnlyList<ForumSanctionDto>,
    PermissionDeniedError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError>>
{
    public async Task<Result<
        IReadOnlyList<ForumSanctionDto>,
        PermissionDeniedError,
        AuthorizationScopeNotFoundError,
        InvalidAuthorizationScopeError>> HandleAsync(
        GetForumSanctionsQuery query,
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
            ForumPolicy.ManageSanctions,
            scope,
            query.EvaluatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var result = query.IncludeInactive
            ? await sanctions.GetHistoryAsync(scope, cancellationToken)
            : await sanctions.GetActiveAsync(scope, query.EvaluatedAt, cancellationToken);
        return result.ToList();
    }
}
