using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetBookmarkedPostsPagedQuerySortType : byte
{
    CreatedAt = 0
}

public sealed class GetBookmarkedPostsPagedQuery<T> : SingleSortPagedQuery<
    Result<IReadOnlyList<T>, NotAdminError>,
    GetBookmarkedPostsPagedQuerySortType
>
{
    public required UserId UserId { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class GetBookmarkedPostsPagedQueryHandler<T> : IQueryHandler<
    GetBookmarkedPostsPagedQuery<T>,
    Result<IReadOnlyList<T>, NotAdminError>
>
{
    private readonly IPostBookmarkReadRepository _repository;

    public GetBookmarkedPostsPagedQueryHandler(IPostBookmarkReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<T>, NotAdminError>> HandleAsync(
        GetBookmarkedPostsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        if (query.UserId != query.RequestedBy.UserId && query.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var posts = await _repository.GetBookmarkedPostsAsync(
            query,
            cancellationToken
        );

        return posts;
    }
}
