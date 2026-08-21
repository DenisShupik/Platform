using System.Globalization;
using System.Linq.Expressions;
using Shared.Infrastructure.Generator;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using Npgsql.NameTranslation;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Persistence.Abstractions;

namespace NotificationService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetThreadSubscriptionLatestEventsPagedQuery<>),
    typeof(SqlKeyValue<ThreadId, NotifiableEvent>))]
[GenerateApplySort(typeof(GetThreadSubscriptionsPagedQuery), typeof(ThreadSubscription))]
internal static partial class ThreadSubscriptionReadRepositoryExtensions
{
    [SortExpression<GetThreadSubscriptionsPagedQuerySortType>(GetThreadSubscriptionsPagedQuerySortType.ThreadId)]
    private static readonly Expression<Func<ThreadSubscription, ThreadId>> ThreadSubscriptionThreadIdExpression =
        e => e.ThreadId;

    [SortExpression<GetThreadSubscriptionLatestEventsPagedQuerySortType>(GetThreadSubscriptionLatestEventsPagedQuerySortType
        .ThreadId)]
    private static readonly Expression<Func<SqlKeyValue<ThreadId, NotifiableEvent>, ThreadId>> ThreadIdExpression =
        e => e.Key;

    [SortExpression<GetThreadSubscriptionLatestEventsPagedQuerySortType>(GetThreadSubscriptionLatestEventsPagedQuerySortType
        .LatestEvent)]
    private static readonly Expression<Func<SqlKeyValue<ThreadId, NotifiableEvent>, DateTime>> LatestEventExpression =
        e => e.Value.OccurredAt;
}

public sealed class ThreadSubscriptionReadRepository : IThreadSubscriptionReadRepository
{
    private static readonly string PostThreadIdColumnName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(
        nameof(IPostNotifiableEventPayload.ThreadId),
        CultureInfo.InvariantCulture);

    private readonly ReadApplicationDbContext _dbContext;

    public ThreadSubscriptionReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken)
    {
        return _dbContext.ThreadSubscriptions
            .AnyAsyncEF(e => e.UserId == userId && e.ThreadId == threadId, cancellationToken);
    }

    public Task<bool> ExistsExcludingUserAsync(ThreadId threadId, UserId? userId, CancellationToken cancellationToken)
    {
        return _dbContext.ThreadSubscriptions
            .AnyAsyncEF(e => e.ThreadId == threadId && (userId == null || e.UserId != userId), cancellationToken);
    }

    public async Task<PagedList<ThreadId>> GetSubscribedThreadIdsAsync(GetThreadSubscriptionsPagedQuery query,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken)
    {
        var projections = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == query.UserId && readableThreadIds.Contains(e.ThreadId))
            .ApplySort(query)
            .Select(e => new
            {
                e.ThreadId,
                TotalCount = Sql.Ext.Count(1).Over().ToValue()
            })
            .ApplyPagination(query)
            .ToListAsyncLinqToDB(cancellationToken);

        var totalCount = projections.FirstOrDefault()?.TotalCount;

        return new PagedList<ThreadId>
        {
            Items = projections.Select(e => e.ThreadId).ToList(),
            TotalCount = totalCount == null ? Count.Default : Count.From(totalCount.Value)
        };
    }

    public async Task<IReadOnlySet<ThreadId>> GetAllSubscribedThreadIdsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var threadIds = await _dbContext.ThreadSubscriptions
            .Where(subscription => subscription.UserId == userId)
            .Select(subscription => subscription.ThreadId)
            .ToListAsyncLinqToDB(cancellationToken);
        return threadIds.ToHashSet();
    }

    public async Task<List<T>> GetLatestEventsAsync<T>(GetThreadSubscriptionLatestEventsPagedQuery<T> query,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken) where T : IThreadEventProjection
    {
        var queryable =
            from ts in _dbContext.ThreadSubscriptions.Where(e =>
                e.UserId == query.UserId && readableThreadIds.Contains(e.ThreadId))
            from ne in _dbContext.NotifiableEvents
                .Where(e => Sql.Property<ThreadId?>(e, PostThreadIdColumnName) == ts.ThreadId)
                .OrderByDescending(e => e.OccurredAt)
                .ThenByDescending(e => e.NotifiableEventId)
                .Take(1)
            select new SqlKeyValue<ThreadId, NotifiableEvent>
            {
                Key = ts.ThreadId,
                Value = ne
            };

        var result = await queryable
            .ApplySort(query)
            .Select(e => e.Value)
            .ProjectToType<T>()
            .ApplyPagination(query)
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}
