using CoreService.Domain.ValueObjects;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IUserNotificationRepository
{
    public Task BulkAddAsync(NotificationId notificationId, ThreadId threadId, UserId userId,
        CancellationToken cancellationToken);
}