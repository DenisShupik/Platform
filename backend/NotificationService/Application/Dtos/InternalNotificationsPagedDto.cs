using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Dtos;

public sealed class InternalNotificationsPagedDto
{
    public IReadOnlyList<InternalNotificationDto> Notifications { get; set; }
    public Dictionary<ThreadId, ThreadTitle> Threads { get; set; }
    public Dictionary<UserId, Username> Users { get; set; }

    /// <summary>
    /// Общее количество уведомлений с учетом фильтрации
    /// </summary>
    public ulong TotalCount { get; set; }
}