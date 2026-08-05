using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using NotificationService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetThreadSubscriptionsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class GetThreadSubscriptionsPagedQuery : SingleSortPagedQuery<
    Result<PagedList<ThreadDto>, NotAdminError>,
    GetThreadSubscriptionsPagedQuerySortType
>
{
    public required UserId UserId { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionsPagedQueryHandler : IQueryHandler<
    GetThreadSubscriptionsPagedQuery,
    Result<PagedList<ThreadDto>, NotAdminError>
>
{
    private readonly IThreadSubscriptionReadRepository _repository;
    private readonly ICoreServiceClient _coreServiceClient;

    public GetThreadSubscriptionsPagedQueryHandler(
        IThreadSubscriptionReadRepository repository,
        ICoreServiceClient coreServiceClient
    )
    {
        _repository = repository;
        _coreServiceClient = coreServiceClient;
    }

    public async Task<Result<PagedList<ThreadDto>, NotAdminError>> HandleAsync(
        GetThreadSubscriptionsPagedQuery query,
        CancellationToken cancellationToken
    )
    {
        if (query.UserId != query.RequestedBy.UserId && query.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var subscribedThreadIds = await _repository.GetSubscribedThreadIdsAsync(query, cancellationToken);
        var threadsById =
            (await _coreServiceClient.GetThreadsAsync(subscribedThreadIds.Items.ToHashSet(), cancellationToken))
            .ToDictionary(e => e.ThreadId);

        return new PagedList<ThreadDto>
        {
            Items = subscribedThreadIds.Items.Select(threadId => threadsById[threadId]).ToList(),
            TotalCount = subscribedThreadIds.TotalCount
        };
    }
}
