using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.UseCases;

[IncludeAsRequired(typeof(UserNotification), nameof(UserNotification.UserId))]
public sealed partial class GetUserNotificationCountQuery
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }

    /// <summary>
    /// Фильтр по типу канала доставки уведомления
    /// </summary>
    public required ChannelType? Channel { get; init; }
}

public sealed class GetUserNotificationCountQueryHandler
{
    private readonly IUserNotificationReadRepository _userNotificationReadRepository;

    public GetUserNotificationCountQueryHandler(
        IUserNotificationReadRepository userNotificationReadRepository
    )
    {
        _userNotificationReadRepository = userNotificationReadRepository;
    }

    public Task<int> HandleAsync(GetUserNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        return _userNotificationReadRepository.GetCountAsync(request.UserId, request.IsDelivered, request.Channel,
            cancellationToken);
    }
}