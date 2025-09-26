using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId))]
public sealed partial class GetInternalNotificationCountQuery: IQuery<ulong>
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }
}

public sealed class GetInternalNotificationCountQueryHandler:  IQueryHandler<GetInternalNotificationCountQuery, ulong>
{
    private readonly INotificationReadRepository _notificationReadRepository;

    public GetInternalNotificationCountQueryHandler(
        INotificationReadRepository notificationReadRepository
    )
    {
        _notificationReadRepository = notificationReadRepository;
    }

    public Task<ulong> HandleAsync(GetInternalNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        return _notificationReadRepository.GetCountAsync(query.UserId, query.IsDelivered, ChannelType.Internal,
            cancellationToken);
    }
}