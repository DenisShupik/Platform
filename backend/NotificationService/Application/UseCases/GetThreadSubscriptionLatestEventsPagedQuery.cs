using NotificationService.Application.Authorization;
using NotificationService.Application.Dtos;
using Shared.Domain.Abstractions.Results;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetThreadSubscriptionLatestEventsPagedQuerySortType : byte
{
    LatestEvent = 0,
    ThreadId = 1
}

public sealed class GetThreadSubscriptionLatestEventsPagedQuery<T> : SingleSortPagedQuery<
    Result<IReadOnlyList<T>, PermissionDeniedError>,
    GetThreadSubscriptionLatestEventsPagedQuerySortType
>
    where T : IThreadEventProjection
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionLatestEventsPagedQueryHandler<T> : IQueryHandler<
    GetThreadSubscriptionLatestEventsPagedQuery<T>,
    Result<IReadOnlyList<T>, PermissionDeniedError>
>
    where T : IThreadEventProjection
{
    private readonly IThreadSubscriptionReadRepository _repository;
    private readonly IThreadAccessReader _threadAccessReader;
    private readonly IThreadSubscriptionPolicyEvaluator _policyEvaluator;

    public GetThreadSubscriptionLatestEventsPagedQueryHandler(
        IThreadSubscriptionReadRepository repository,
        IThreadAccessReader threadAccessReader,
        IThreadSubscriptionPolicyEvaluator policyEvaluator)
    {
        _repository = repository;
        _threadAccessReader = threadAccessReader;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<Result<IReadOnlyList<T>, PermissionDeniedError>> HandleAsync(
        GetThreadSubscriptionLatestEventsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        var authorization = _policyEvaluator.Authorize(
            query.RequestedBy,
            ThreadSubscriptionPolicy.Read,
            query.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var threadIds = await _repository.GetAllSubscribedThreadIdsAsync(query.UserId, cancellationToken);
        var readableThreadIds = (await _threadAccessReader.GetReadableAsync(
                threadIds, query.RequestedBy.UserId, cancellationToken))
            .Select(thread => thread.ThreadId)
            .ToHashSet();

        return await _repository.GetLatestEventsAsync(query, readableThreadIds, cancellationToken);
    }
}
