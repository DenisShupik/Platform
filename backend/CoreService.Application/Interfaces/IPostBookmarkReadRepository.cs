using CoreService.Domain.ValueObjects;
using CoreService.Application.UseCases;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPostBookmarkReadRepository
{
    Task<List<T>> GetBookmarkedPostsAsync<T>(
        GetBookmarkedPostsPagedQuery<T> query,
        CancellationToken cancellationToken
    );

    Task<Count> GetBookmarkedPostsCountAsync(
        GetBookmarkedPostsCountQuery query,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<PostId>> GetBookmarkedPostIdsBulkAsync(
        UserId userId,
        IdSet<PostId, Guid> postIds,
        CancellationToken cancellationToken
    );
}
