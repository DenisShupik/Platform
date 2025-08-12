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
}