using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetBookmarkedPostsCountQuery : IQuery<Result<Count, PermissionDeniedError>>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class GetBookmarkedPostsCountQueryHandler : IQueryHandler<
    GetBookmarkedPostsCountQuery,
    Result<Count, PermissionDeniedError>
>
{
    private readonly IPostBookmarkReadRepository _repository;
    private readonly IBookmarkPolicyEvaluator _policyEvaluator;

    public GetBookmarkedPostsCountQueryHandler(
        IPostBookmarkReadRepository repository,
        IBookmarkPolicyEvaluator policyEvaluator)
    {
        _repository = repository;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<Result<Count, PermissionDeniedError>> HandleAsync(
        GetBookmarkedPostsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        var authorization = _policyEvaluator.Authorize(
            query.RequestedBy,
            BookmarkPolicy.Read,
            query.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var count = await _repository.GetBookmarkedPostsCountAsync(
            query,
            cancellationToken
        );

        return count;
    }
}
