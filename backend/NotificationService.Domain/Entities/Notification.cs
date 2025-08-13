using Generator.Attributes;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
[Include(typeof(NotifiableEvent), PropertyGenerationMode.AsPrivateSet, nameof(NotifiableEvent.NotifiableEventId))]
public sealed partial class Notification
{
    /// <summary>
    /// Событие
    /// </summary>
    public NotifiableEvent NotifiableEvent { get; private set; }

    /// <summary>
    /// Канал доставки уведомления
    /// </summary>
    public ChannelType Channel { get; private set; }

    /// <summary>
    /// Дата и время доставки уведомления
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    public Notification(UserId userId, NotifiableEventId notifiableEventId, ChannelType channel)
    {
        UserId = userId;
        NotifiableEventId = notifiableEventId;
        Channel = channel;
        DeliveredAt = null;
    }
}