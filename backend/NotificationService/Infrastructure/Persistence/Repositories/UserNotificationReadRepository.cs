using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using SharedKernel.Application.Enums;
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

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetInternalUserNotificationQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserNotifications
            .Include(e => e.Notification)
            .Where(e =>
                e.UserId == request.UserId
                && (request.IsDelivered == null || e.DeliveredAt != null == request.IsDelivered.Value)
                && (request.Channel == null || e.Channel == request.Channel)
            );

        if (
            request.Sort != null && request.Sort.Field is GetInternalUserNotificationQuery.SortType.DeliveryAt
        )
        {
            query = request.Sort.Order == SortOrderType.Ascending
                ? query.OrderBy(e => e.DeliveredAt)
                : query.OrderByDescending(e => e.DeliveredAt);
        }
        else
        {
            query = query
                .OrderBy(e => e.NotificationId);
        }

        var projections = await query
            .ProjectToType<T>()
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsyncEF(cancellationToken);

        return projections;
    }
}