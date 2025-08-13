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
using static NotificationService.Application.UseCases.GetInternalNotificationsPagedQuery.
    GetInternalNotificationQuerySortType;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class NotificationReadRepository : INotificationReadRepository
{
    private readonly ReadonlyApplicationDbContext _dbContext;

    private static readonly Expression<Func<Notification, DateTime>> _occurredAtExpr =
        e => e.NotifiableEvent.OccurredAt;

    private static readonly Expression<Func<Notification, DateTime?>> _deliveredAtExpr = e => e.DeliveredAt;

    public NotificationReadRepository(ReadonlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken)
    {
        return _dbContext.Notifications
            .Where(e =>
                e.UserId == userId
                && (isDelivered == null || e.DeliveredAt != null == isDelivered.Value)
                && (channel == null || e.Channel == channel)
            )
            .CountAsyncLinqToDB(cancellationToken);
    }

    public async Task<PagedList<T>> GetAllAsync<T>(GetInternalNotificationsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Include(e => e.NotifiableEvent)
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
                .OrderBy(e => e.NotifiableEvent.OccurredAt);
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