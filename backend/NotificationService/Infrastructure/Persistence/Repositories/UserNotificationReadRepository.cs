using LinqToDB.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationReadRepository : IUserNotificationReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserNotificationReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserNotifications
            .Where(e =>
                e.UserId == userId
                && (isDelivered == null || e.DeliveredAt != null == isDelivered.Value)
                && (channel == null || e.Channel == channel)
            )
            .CountAsyncLinqToDB(cancellationToken);
    }
}