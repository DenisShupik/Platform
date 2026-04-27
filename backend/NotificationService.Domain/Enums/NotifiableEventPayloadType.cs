namespace NotificationService.Domain.Enums;

public enum NotifiableEventPayloadType : byte
{
    /// <summary>
    /// Уведомление о добавлении нового поста
    /// </summary>
    PostAdded = 0,

    /// <summary>
    /// Уведомление об обновлении поста
    /// </summary>
    PostUpdated = 1,

    /// <summary>
    /// Уведомление о том что тема прошла модерацию
    /// </summary>
    ThreadApproved = 2,

    /// <summary>
    /// Уведомление о том что тема отклонена модератором
    /// </summary>
    ThreadRejected = 3,
}