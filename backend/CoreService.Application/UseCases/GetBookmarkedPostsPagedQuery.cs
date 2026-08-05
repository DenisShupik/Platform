using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetBookmarkedPostsPagedQuerySortType : byte
{
    CreatedAt = 0
}

public sealed class GetBookmarkedPostsPagedQuery<T> : SingleSortPagedQuery<
    IReadOnlyList<T>,
    GetBookmarkedPostsPagedQuerySortType
>
{
    public required UserIdRole QueriedBy { get; init; }
}

public sealed class GetBookmarkedPostsPagedQueryHandler<T> : IQueryHandler<
    GetBookmarkedPostsPagedQuery<T>,
    IReadOnlyList<T>
>
{
    private readonly IPostBookmarkReadRepository _repository;

    public GetBookmarkedPostsPagedQueryHandler(IPostBookmarkReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(
        GetBookmarkedPostsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetBookmarkedPostsAsync(query, cancellationToken);
    }
}
