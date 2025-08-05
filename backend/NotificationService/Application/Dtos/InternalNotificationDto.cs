using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using NotificationService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Dtos;

[Include(typeof(Notification), PropertyGenerationMode.AsPrivateSet, nameof(Notification.Payload),
    nameof(Notification.OccurredAt))]
[Include(typeof(UserNotification), PropertyGenerationMode.AsPrivateSet, nameof(UserNotification.NotificationId),
    nameof(UserNotification.DeliveredAt))]
public sealed partial class InternalUserNotificationDto;

public sealed class InternalUserNotificationsDto
{
    public IReadOnlyList<InternalUserNotificationDto> Notifications { get; set; }
    public Dictionary<ThreadId, ThreadTitle> Threads { get; set; }
    public Dictionary<UserId, string> Users { get; set; }

    /// <summary>
    /// Общее количество уведомлений
    /// </summary>
    public long TotalCount { get; set; }
}