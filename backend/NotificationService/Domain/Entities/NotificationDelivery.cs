using Generator.Attributes;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

[Include(typeof(Notification), nameof(Notification.NotificationId))]
[Include(typeof(User), nameof(User.UserId))]
public sealed partial class NotificationDelivery
{
    /// <summary>
    /// Канал доставки уведомления
    /// </summary>
    public ChannelType Channel { get; private set; }

    /// <summary>
    /// Дата и время доставки уведомления
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }

    public NotificationDelivery(NotificationId notificationId, UserId userId, ChannelType channel)
    {
        NotificationId = notificationId;
        UserId = userId;
        Channel = channel;
        DeliveredAt = null;
    }
}