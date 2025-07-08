using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    public Task AddAsync(Notification notification, CancellationToken cancellationToken);
}