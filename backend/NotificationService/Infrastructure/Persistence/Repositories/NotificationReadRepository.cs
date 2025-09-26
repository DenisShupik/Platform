using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetInternalNotificationsPagedQuery), typeof(Notification))]
internal static partial class NotificationReadRepositoryExtensions
{
    [SortExpression<GetInternalNotificationsPagedQuerySortType>(GetInternalNotificationsPagedQuerySortType
        .OccurredAt)]
    private static readonly Expression<Func<Notification, DateTime>> OccurredAtExpression =
        e => e.NotifiableEvent.OccurredAt;
}

public sealed class NotificationReadRepository : INotificationReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public NotificationReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ulong> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken)
    {
        return (ulong)await _dbContext.Notifications
            .Where(e =>
                e.UserId == userId
                && (isDelivered == null || e.DeliveredAt != null == isDelivered.Value)
                && (channel == null || e.Channel == channel)
            )
            .LongCountAsyncLinqToDB(cancellationToken);
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
            )
            .ApplySort(request)
            .ProjectToType<T>()
            .Select(e => new
            {
                Notificatiion = e,
                TotalCount = Sql.Ext.LongCount(1).Over().ToValue()
            })
            .ApplyPagination(request)
            .ToListAsyncLinqToDB(cancellationToken);

        var projections = await query;

        return new PagedList<T>
        {
            Items = projections.Select(e => e.Notificatiion).ToList(),
            TotalCount = (ulong?)projections.FirstOrDefault()?.TotalCount ?? 0
        };
    }
}