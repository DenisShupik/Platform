using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using UserService.Domain.ValueObjects;
using static NotificationService.Application.UseCases.GetInternalUserNotificationQuery.
    GetInternalUserNotificationQuerySortType;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationReadRepository : IUserNotificationReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly Expression<Func<UserNotification, DateTime>> _occurredAtExpr =
        e => e.Notification.OccurredAt;

    private static readonly Expression<Func<UserNotification, DateTime?>> _deliveredAtExpr = e => e.DeliveredAt;

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

    public async Task<PagedList<T>> GetAllAsync<T>(GetInternalUserNotificationQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserNotifications
            .Include(e => e.Notification)
            .Where(e =>
                e.UserId == request.UserId
                && e.Channel == ChannelType.Internal
                && (request.IsDelivered == null || e.DeliveredAt != null == request.IsDelivered.Value)
            );

        if (request.Sort != null)
        {
            var isFirst = true;
            foreach (var sortCriteria in request.Sort)
            {
                query = sortCriteria.Field switch
                {
                    OccurredAt => query.ApplySort(_occurredAtExpr, sortCriteria.Order, isFirst),
                    DeliveredAt => query.ApplySort(_deliveredAtExpr, sortCriteria.Order, isFirst)
                };

                isFirst = false;
            }
        }
        else
        {
            query = query
                .OrderBy(e => e.Notification.OccurredAt);
        }

        var projections = await query
            .ProjectToType<T>()
            .Select(e => new
            {
                Notificatiion = e,
                TotalCount = Sql.Ext.Count(1).Over().ToValue()
            })
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsyncLinqToDB(cancellationToken);

        return new PagedList<T>
        {
            Items = projections.Select(e => e.Notificatiion).ToList(),
            TotalCount = projections.FirstOrDefault()?.TotalCount ?? 0
        };
    }
}

public static class IQueryableExtensions
{
    public static IOrderedQueryable<T> ApplySort<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        SortOrderType sortOrder,
        bool isFirst)
    {
        var ascending = sortOrder == SortOrderType.Ascending;
        if (isFirst)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        var ordered = (IOrderedQueryable<T>)source;
        return ascending
            ? ordered.ThenBy(keySelector)
            : ordered.ThenByDescending(keySelector);
    }
}