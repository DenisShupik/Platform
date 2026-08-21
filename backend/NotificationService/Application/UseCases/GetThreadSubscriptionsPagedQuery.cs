using NotificationService.Application.Authorization;
using NotificationService.Application.Dtos;
using Shared.Domain.Abstractions.Results;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Errors;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetThreadSubscriptionsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class GetThreadSubscriptionsPagedQuery : SingleSortPagedQuery<
    Result<PagedList<ThreadSummaryDto>, PermissionDeniedError>,
    GetThreadSubscriptionsPagedQuerySortType
>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionsPagedQueryHandler : IQueryHandler<
    GetThreadSubscriptionsPagedQuery,
    Result<PagedList<ThreadSummaryDto>, PermissionDeniedError>
>
{
    private readonly IThreadSubscriptionReadRepository _repository;
    private readonly IThreadAccessReader _threadAccessReader;
    private readonly IThreadSubscriptionPolicyEvaluator _policyEvaluator;

    public GetThreadSubscriptionsPagedQueryHandler(
        IThreadSubscriptionReadRepository repository,
        IThreadAccessReader threadAccessReader,
        IThreadSubscriptionPolicyEvaluator policyEvaluator
    )
    {
        _repository = repository;
        _threadAccessReader = threadAccessReader;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<Result<PagedList<ThreadSummaryDto>, PermissionDeniedError>> HandleAsync(
        GetThreadSubscriptionsPagedQuery query,
        CancellationToken cancellationToken
    )
    {
        var authorization = _policyEvaluator.Authorize(
            query.RequestedBy,
            ThreadSubscriptionPolicy.Read,
            query.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var allSubscribedThreadIds = await _repository.GetAllSubscribedThreadIdsAsync(
            query.UserId,
            cancellationToken);
        var readableThreads = await _threadAccessReader.GetReadableAsync(
            allSubscribedThreadIds,
            query.RequestedBy.UserId,
            cancellationToken);
        var threadsById = readableThreads.ToDictionary(thread => thread.ThreadId);
        var subscribedThreadIds = await _repository.GetSubscribedThreadIdsAsync(
            query,
            threadsById.Keys.ToHashSet(),
            cancellationToken);
        var visibleThreads = subscribedThreadIds.Items.Select(threadId => threadsById[threadId]).ToList();

        return new PagedList<ThreadSummaryDto>
        {
            Items = visibleThreads,
            TotalCount = subscribedThreadIds.TotalCount
        };
    }
}
