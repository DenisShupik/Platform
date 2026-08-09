using System.Linq.Expressions;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence.Extensions;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
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
        CancellationToken cancellationToken)
    {
        var projections = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == query.UserId)
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

    public async Task<List<T>> GetLatestEventsAsync<T>(GetThreadSubscriptionLatestEventsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = (
            from ts in _dbContext.ThreadSubscriptions.Where(e => e.UserId == query.UserId)
            from ne in _dbContext.NotifiableEvents.Where(e => e.Payload.IsPostEventFor(ts.ThreadId))
            select new SqlKeyValue<ThreadId, NotifiableEvent>
            {
                Key = ts.ThreadId,
                Value = ne
            })
            .OrderBy(e => e.Key)
            .ThenByDescending(e => e.Value.OccurredAt)
            .DistinctBy(e => e.Key);

        var result = await queryable
            .ApplySort(query)
            .Select(e => e.Value)
            .ProjectToType<T>()
            .ApplyPagination(query)
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}
