using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

using QueryResult = Count;

public sealed class GetInternalNotificationCountQuery: IQuery<QueryResult>
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }

    public required UserIdRole QueriedBy { get; init; }
}

public sealed class GetInternalNotificationCountQueryHandler:  IQueryHandler<GetInternalNotificationCountQuery, QueryResult>
{
    private readonly INotificationReadRepository _notificationReadRepository;

    public GetInternalNotificationCountQueryHandler(
        INotificationReadRepository notificationReadRepository
    )
    {
        _notificationReadRepository = notificationReadRepository;
    }

    public Task<QueryResult> HandleAsync(GetInternalNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        return _notificationReadRepository.GetCountAsync(query.QueriedBy.UserId, query.IsDelivered, ChannelType.Internal,
            cancellationToken);
    }
}