using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class NotificationDeliveryRepository : INotificationDeliveryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationDeliveryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BulkAddThreadNotificationDeliveryAsync(NotificationId notificationId, ThreadId threadId,
        UserId userId, CancellationToken cancellationToken)
    {
        await using var dataContext = _dbContext.CreateLinqToDBContext();
        await (
                from ts in _dbContext.ThreadSubscriptions
                    .Where(e => e.ThreadId == threadId && e.UserId != userId)
                from c in dataContext.Unnest(ts.Channels)
                select new { ts.UserId, Channel = c }
            )
            .Into(_dbContext.NotificationDeliveries.ToLinqToDBTable())
            .Value(e => e.NotificationId, notificationId)
            .Value(e => e.UserId, s => s.UserId)
            .Value(e => e.Channel, s => s.Channel)
            .Value(e => e.DeliveredAt, (DateTime?)null)
            .InsertAsync(cancellationToken);
    }
}