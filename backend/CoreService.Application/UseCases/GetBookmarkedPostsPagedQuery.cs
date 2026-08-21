using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetBookmarkedPostsPagedQuerySortType : byte
{
    CreatedAt = 0
}

public sealed class GetBookmarkedPostsPagedQuery<T> : SingleSortPagedQuery<
    Result<IReadOnlyList<T>, PermissionDeniedError>,
    GetBookmarkedPostsPagedQuerySortType
>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class GetBookmarkedPostsPagedQueryHandler<T> : IQueryHandler<
    GetBookmarkedPostsPagedQuery<T>,
    Result<IReadOnlyList<T>, PermissionDeniedError>
>
{
    private readonly IPostBookmarkReadRepository _repository;
    private readonly IBookmarkPolicyEvaluator _policyEvaluator;

    public GetBookmarkedPostsPagedQueryHandler(
        IPostBookmarkReadRepository repository,
        IBookmarkPolicyEvaluator policyEvaluator)
    {
        _repository = repository;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<Result<IReadOnlyList<T>, PermissionDeniedError>> HandleAsync(
        GetBookmarkedPostsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        var authorization = _policyEvaluator.Authorize(
            query.RequestedBy,
            BookmarkPolicy.Read,
            query.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var posts = await _repository.GetBookmarkedPostsAsync(
            query,
            cancellationToken
        );

        return posts;
    }
}
