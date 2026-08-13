using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetBookmarkedPostIdsBulkQuery : IQuery<IReadOnlyList<PostId>>
{
    public required UserId QueriedBy { get; init; }
    public required IdSet<PostId, Guid> PostIds { get; init; }
}

public sealed class GetBookmarkedPostIdsBulkQueryHandler :
    IQueryHandler<GetBookmarkedPostIdsBulkQuery, IReadOnlyList<PostId>>
{
    private readonly IPostBookmarkReadRepository _postBookmarkReadRepository;

    public GetBookmarkedPostIdsBulkQueryHandler(IPostBookmarkReadRepository postBookmarkReadRepository)
    {
        _postBookmarkReadRepository = postBookmarkReadRepository;
    }

    public Task<IReadOnlyList<PostId>> HandleAsync(
        GetBookmarkedPostIdsBulkQuery query,
        CancellationToken cancellationToken
    )
    {
        return _postBookmarkReadRepository.GetBookmarkedPostIdsBulkAsync(
            query.QueriedBy,
            query.PostIds,
            cancellationToken
        );
    }
}
