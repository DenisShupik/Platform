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
    public NotificationData Data { get; private set; }

    public Notification(NotificationData data)
    {
        NotificationId = NotificationId.From(Guid.CreateVersion7());
        Data = data;
    }
}