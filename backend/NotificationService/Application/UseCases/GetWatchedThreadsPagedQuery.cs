using CoreService.Domain.ValueObjects;
using NotificationService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetWatchedThreadsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class GetWatchedThreadsPagedQuery<T> : SingleSortPagedQuery<
    IReadOnlyList<T>,
    GetWatchedThreadsPagedQuerySortType
>
{
    public required UserId QueriedBy { get; init; }
}

public sealed class GetWatchedThreadsPagedQueryHandler<T> : IQueryHandler<
    GetWatchedThreadsPagedQuery<T>,
    IReadOnlyList<T>
>
{
    private readonly IThreadSubscriptionReadRepository _repository;

    public GetWatchedThreadsPagedQueryHandler(IThreadSubscriptionReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(
        GetWatchedThreadsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetWatchedThreadsAsync(query, cancellationToken);
    }
}
