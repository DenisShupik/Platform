using NotificationService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetWatchedThreadLatestEventPagedQuerySortType : byte
{
    LatestEvent = 0,
    ThreadId = 1
}

public sealed class GetWatchedThreadLatestEventPagedQuery<T> : SingleSortPagedQuery<IReadOnlyList<T>,
    GetWatchedThreadLatestEventPagedQuerySortType>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetWatchedThreadLatestEventPagedQueryHandler<T> : IQueryHandler<GetWatchedThreadLatestEventPagedQuery<T>,
    IReadOnlyList<T>>
{
    private readonly IThreadSubscriptionReadRepository _repository;

    public GetWatchedThreadLatestEventPagedQueryHandler(IThreadSubscriptionReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetWatchedThreadLatestEventPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetLatestEventPerThreadAsync(query, cancellationToken);
    }
}