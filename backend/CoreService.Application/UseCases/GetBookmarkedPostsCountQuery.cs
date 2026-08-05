using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetBookmarkedPostsCountQuery : IQuery<Count>
{
    public required UserIdRole QueriedBy { get; init; }
}

public sealed class GetBookmarkedPostsCountQueryHandler : IQueryHandler<GetBookmarkedPostsCountQuery, Count>
{
    private readonly IPostBookmarkReadRepository _repository;

    public GetBookmarkedPostsCountQueryHandler(IPostBookmarkReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Count> HandleAsync(GetBookmarkedPostsCountQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetBookmarkedPostsCountAsync(query, cancellationToken);
    }
}
