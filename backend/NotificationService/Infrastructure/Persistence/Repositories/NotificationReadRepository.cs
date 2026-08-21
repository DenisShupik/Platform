using System.Linq.Expressions;
using System.Globalization;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Npgsql.NameTranslation;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

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
    private static readonly string ThreadIdColumnName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(
        nameof(IThreadNotifiableEventPayload.ThreadId),
        CultureInfo.InvariantCulture);

    private readonly ReadApplicationDbContext _dbContext;

    public NotificationReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlySet<ThreadId>> GetThreadIdsAsync(
        UserId userId,
        bool? isDelivered,
        ChannelType? channel,
        CancellationToken cancellationToken)
    {
        var ids = await _dbContext.Notifications
            .Where(notification =>
                notification.UserId == userId &&
                (isDelivered == null || notification.DeliveredAt != null == isDelivered.Value) &&
                (channel == null || notification.Channel == channel))
            .Select(notification => Sql.Property<ThreadId?>(
                notification.NotifiableEvent,
                ThreadIdColumnName))
            .Where(threadId => threadId != null)
            .Select(threadId => threadId!.Value)
            .Distinct()
            .ToListAsyncLinqToDB(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task<Count> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken)
    {
        return Count.From(await _dbContext.Notifications
            .Where(e =>
                e.UserId == userId
                && (isDelivered == null || e.DeliveredAt != null == isDelivered.Value)
                && (channel == null || e.Channel == channel)
                && readableThreadIds.Contains(Sql.Property<ThreadId>(e.NotifiableEvent, ThreadIdColumnName))
            )
            .CountAsyncLinqToDB(cancellationToken));
    }

    public async Task<PagedList<T>> GetAllAsync<T>(GetInternalNotificationsPagedQuery request,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken)
    {
        var filtered = _dbContext.Notifications
            .Include(e => e.NotifiableEvent)
            .Where(e =>
                e.UserId == request.UserId
                && e.Channel == ChannelType.Internal
                && (request.IsDelivered == null || e.DeliveredAt != null == request.IsDelivered.Value)
                && readableThreadIds.Contains(Sql.Property<ThreadId>(e.NotifiableEvent, ThreadIdColumnName))
            );
        var totalCount = await filtered.CountAsyncLinqToDB(cancellationToken);
        var items = await filtered
            .ApplySort(request)
            .ProjectToType<T>()
            .ApplyPagination(request)
            .ToListAsyncLinqToDB(cancellationToken);

        return new PagedList<T>
        {
            Items = items,
            TotalCount = Count.From(totalCount)
        };
    }
}
