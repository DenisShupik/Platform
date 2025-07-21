using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using OneOf;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserNotificationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<UserNotification, UserNotificationNotFoundError>> GetOneAsync(UserId userId,
        NotificationId notificationId, ChannelType channel, CancellationToken cancellationToken)
    {
        var userNotification = await _dbContext.UserNotifications
            .Where(e => e.UserId == userId && e.NotificationId == notificationId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (userNotification == null) return new UserNotificationNotFoundError(userId, notificationId, channel);

        return userNotification;
    }

    public async Task BulkAddAsync(NotificationId notificationId, ThreadId threadId,
        UserId userId, CancellationToken cancellationToken)
    {
        await using var dataContext = _dbContext.CreateLinqToDBContext();
        await (
                from ts in _dbContext.ThreadSubscriptions
                    .Where(e => e.ThreadId == threadId && e.UserId != userId)
                from c in dataContext.Unnest(ts.Channels)
                select new { ts.UserId, Channel = c }
            )
            .Into(_dbContext.UserNotifications.ToLinqToDBTable())
            .Value(e => e.NotificationId, notificationId)
            .Value(e => e.UserId, s => s.UserId)
            .Value(e => e.Channel, s => s.Channel)
            .Value(e => e.DeliveredAt, (DateTime?)null)
            .InsertAsync(cancellationToken);
    }
}