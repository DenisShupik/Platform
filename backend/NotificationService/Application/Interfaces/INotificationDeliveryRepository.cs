using CoreService.Domain.ValueObjects;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface INotificationDeliveryRepository
{
    public Task BulkAddThreadNotificationDeliveryAsync(NotificationId notificationId, ThreadId threadId, UserId userId, CancellationToken cancellationToken);
}