using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

public sealed class Notification
{
    /// <summary>
    /// Идентификатор уведомления
    /// </summary>
    public NotificationId NotificationId { get; private set; }

    /// <summary>
    /// Данные уведомления
    /// </summary>
    public NotificationPayload Payload { get; private set; }

    /// <summary>
    /// Дата и время события, породившего уведомление
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    public Notification(NotificationPayload payload, DateTime occurredAt)
    {
        NotificationId = NotificationId.From(Guid.CreateVersion7());
        Payload = payload;
        OccurredAt = occurredAt;
    }
}