using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId))]
public sealed partial class GetInternalNotificationCountQuery
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }
}

public sealed class GetInternalNotificationCountQueryHandler
{
    private readonly INotificationReadRepository _notificationReadRepository;

    public GetInternalNotificationCountQueryHandler(
        INotificationReadRepository notificationReadRepository
    )
    {
        _notificationReadRepository = notificationReadRepository;
    }

    public Task<int> HandleAsync(GetInternalNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        return _notificationReadRepository.GetCountAsync(request.UserId, request.IsDelivered, ChannelType.Internal,
            cancellationToken);
    }
}