using System.Runtime.CompilerServices;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserNotificationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IQueryable<UserNotification> GetUserNotificationQuery(UserId userId, NotificationId notificationId,
        ChannelType channel)
    {
        return _dbContext.UserNotifications
            .Where(e => e.UserId == userId && e.NotificationId == notificationId && e.Channel == channel);
    }

    public async Task<OneOf<UserNotification, UserNotificationNotFoundError>> GetOneAsync(UserId userId,
        NotificationId notificationId, ChannelType channel, CancellationToken cancellationToken)
    {
        var userNotification = await GetUserNotificationQuery(userId, notificationId, channel)
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

    public async Task<OneOf<Success, UserNotificationNotFoundError>> ExecuteRemoveAsync(UserId userId,
        NotificationId notificationId, ChannelType channel, CancellationToken cancellationToken)
    {
        var deletedCount = await GetUserNotificationQuery(userId, notificationId, channel)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount == 0)
            return new UserNotificationNotFoundError(userId, notificationId, ChannelType.Internal);

        return OneOfHelper.Success;
    }
}