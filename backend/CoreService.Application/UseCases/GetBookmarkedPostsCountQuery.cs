using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetBookmarkedPostsCountQuery : IQuery<Result<Count, NotAdminError>>
{
    public required UserId UserId { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class GetBookmarkedPostsCountQueryHandler : IQueryHandler<
    GetBookmarkedPostsCountQuery,
    Result<Count, NotAdminError>
>
{
    private readonly IPostBookmarkReadRepository _repository;

    public GetBookmarkedPostsCountQueryHandler(IPostBookmarkReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Count, NotAdminError>> HandleAsync(
        GetBookmarkedPostsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        if (query.UserId != query.RequestedBy.UserId && query.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var count = await _repository.GetBookmarkedPostsCountAsync(
            query,
            cancellationToken
        );

        return count;
    }
}
