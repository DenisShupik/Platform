using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;

namespace NotificationService.Application.Interfaces;

public interface IUserNotificationRepository
{
    public Task<OneOf<UserNotification, UserNotificationNotFoundError>> GetOneAsync(UserId userId,
        NotificationId notificationId, ChannelType channel, CancellationToken cancellationToken);

    public Task BulkAddAsync(NotificationId notificationId, ThreadId threadId, UserId userId,
        CancellationToken cancellationToken);

    public Task<OneOf<Success, UserNotificationNotFoundError>> ExecuteRemoveAsync(UserId userId,
        NotificationId notificationId, ChannelType channel, CancellationToken cancellationToken);
}