using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using NotificationService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetWatchedThreadsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class GetWatchedThreadsPagedQuery : SingleSortPagedQuery<
    PagedList<ThreadDto>,
    GetWatchedThreadsPagedQuerySortType
>
{
    public required UserId QueriedBy { get; init; }
}

public sealed class GetWatchedThreadsPagedQueryHandler : IQueryHandler<
    GetWatchedThreadsPagedQuery,
    PagedList<ThreadDto>
>
{
    private readonly IThreadSubscriptionReadRepository _repository;
    private readonly ICoreServiceClient _coreServiceClient;

    public GetWatchedThreadsPagedQueryHandler(
        IThreadSubscriptionReadRepository repository,
        ICoreServiceClient coreServiceClient
    )
    {
        _repository = repository;
        _coreServiceClient = coreServiceClient;
    }

    public async Task<PagedList<ThreadDto>> HandleAsync(
        GetWatchedThreadsPagedQuery query,
        CancellationToken cancellationToken
    )
    {
        var watchedThreads = await _repository.GetWatchedThreadsAsync(query, cancellationToken);
        var threadsById = (await _coreServiceClient.GetThreadsAsync(watchedThreads.Items.ToHashSet(), cancellationToken))
            .ToDictionary(e => e.ThreadId);

        return new PagedList<ThreadDto>
        {
            Items = watchedThreads.Items.Select(threadId => threadsById[threadId]).ToList(),
            TotalCount = watchedThreads.TotalCount
        };
    }
}
