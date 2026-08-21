using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

using QueryResult = Count;

public sealed class GetInternalNotificationCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }

    public required ActorContext QueriedBy { get; init; }
}

public sealed class GetInternalNotificationCountQueryHandler : IQueryHandler<GetInternalNotificationCountQuery, QueryResult>
{
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly IThreadAccessReader _threadAccessReader;

    public GetInternalNotificationCountQueryHandler(
        INotificationReadRepository notificationReadRepository,
        IThreadAccessReader threadAccessReader
    )
    {
        _notificationReadRepository = notificationReadRepository;
        _threadAccessReader = threadAccessReader;
    }

    public async Task<QueryResult> HandleAsync(GetInternalNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        var threadIds = await _notificationReadRepository.GetThreadIdsAsync(
            query.QueriedBy.UserId,
            query.IsDelivered,
            ChannelType.Internal,
            cancellationToken);
        var readableThreadIds = (await _threadAccessReader.GetReadableAsync(
                threadIds,
                query.QueriedBy.UserId,
                cancellationToken))
            .Select(thread => thread.ThreadId)
            .ToHashSet();

        return await _notificationReadRepository.GetCountAsync(
            query.QueriedBy.UserId,
            query.IsDelivered,
            ChannelType.Internal,
            readableThreadIds,
            cancellationToken);
    }
}
