using NotificationService.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public sealed class GetWatchedThreadsCountQuery : IQuery<Count>
{
    public required UserId QueriedBy { get; init; }
}

public sealed class GetWatchedThreadsCountQueryHandler : IQueryHandler<GetWatchedThreadsCountQuery, Count>
{
    private readonly IThreadSubscriptionReadRepository _repository;

    public GetWatchedThreadsCountQueryHandler(IThreadSubscriptionReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Count> HandleAsync(GetWatchedThreadsCountQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetWatchedThreadsCountAsync(query, cancellationToken);
    }
}
