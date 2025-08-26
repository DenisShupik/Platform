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

public sealed class NotificationWriteRepository : INotificationWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public NotificationWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IQueryable<Notification> GetNotificationQuery(UserId userId, NotifiableEventId notifiableEventId,
        ChannelType channel)
    {
        return _dbContext.Notifications
            .Where(e => e.UserId == userId && e.NotifiableEventId == notifiableEventId && e.Channel == channel);
    }

    public async Task<OneOf<Notification, NotificationNotFoundError>> GetOneAsync(UserId userId,
        NotifiableEventId notifiableEventId, ChannelType channel, CancellationToken cancellationToken)
    {
        var notification = await GetNotificationQuery(userId, notifiableEventId, channel)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (notification == null) return new NotificationNotFoundError(userId, notifiableEventId, channel);

        return notification;
    }

    public async Task BulkAddAsync(NotifiableEventId notifiableEventId, ThreadId threadId,
        UserId userId, CancellationToken cancellationToken)
    {
        await using var dataContext = _dbContext.CreateLinqToDBContext();
        await (
                from ts in _dbContext.ThreadSubscriptions
                    .Where(e => e.ThreadId == threadId && e.UserId != userId)
                from c in dataContext.Unnest(ts.Channels)
                select new { ts.UserId, Channel = c }
            )
            .Into(_dbContext.Notifications.ToLinqToDBTable())
            .Value(e => e.NotifiableEventId, notifiableEventId)
            .Value(e => e.UserId, s => s.UserId)
            .Value(e => e.Channel, s => s.Channel)
            .Value(e => e.DeliveredAt, (DateTime?)null)
            .InsertAsync(cancellationToken);
    }

    public async Task<OneOf<Success, NotificationNotFoundError>> ExecuteRemoveAsync(UserId userId,
        NotifiableEventId notifiableEventId, ChannelType channel, CancellationToken cancellationToken)
    {
        var deletedCount = await GetNotificationQuery(userId, notifiableEventId, channel)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount == 0)
            return new NotificationNotFoundError(userId, notifiableEventId, ChannelType.Internal);

        return OneOfHelper.Success;
    }
}