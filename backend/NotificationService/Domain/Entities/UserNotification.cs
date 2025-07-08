using Generator.Attributes;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

[Include(typeof(User), nameof(User.UserId))]
[Include(typeof(Notification), nameof(Notification.NotificationId))]
public sealed partial class UserNotification
{
    /// <summary>
    /// Канал доставки уведомления
    /// </summary>
    public ChannelType Channel { get; private set; }

    /// <summary>
    /// Дата и время доставки уведомления
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }

    public UserNotification(UserId userId, NotificationId notificationId, ChannelType channel)
    {
        UserId = userId;
        NotificationId = notificationId;
        Channel = channel;
        DeliveredAt = null;
    }
}