using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetBookmarkedPostIdsQuery : IQuery<IReadOnlyList<PostId>>
{
    public required UserId QueriedBy { get; init; }
    public required IdSet<PostId, Guid> PostIds { get; init; }
}

public sealed class GetBookmarkedPostIdsQueryHandler :
    IQueryHandler<GetBookmarkedPostIdsQuery, IReadOnlyList<PostId>>
{
    private readonly IPostBookmarkReadRepository _postBookmarkReadRepository;

    public GetBookmarkedPostIdsQueryHandler(IPostBookmarkReadRepository postBookmarkReadRepository)
    {
        _postBookmarkReadRepository = postBookmarkReadRepository;
    }

    public Task<IReadOnlyList<PostId>> HandleAsync(
        GetBookmarkedPostIdsQuery query,
        CancellationToken cancellationToken
    )
    {
        return _postBookmarkReadRepository.GetBookmarkedPostIdsAsync(
            query.QueriedBy,
            query.PostIds,
            cancellationToken
        );
    }
}
