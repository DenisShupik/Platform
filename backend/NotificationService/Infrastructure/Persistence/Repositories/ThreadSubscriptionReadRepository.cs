using System.Linq.Expressions;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using NotificationService.Application.Entities;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using static NotificationService.Infrastructure.Persistence.Extensions.QueryableExtensions;

namespace NotificationService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetWatchedThreadLatestEventPagedQuery<>), typeof(WatchedThreadLatestEvent))]
internal static partial class ThreadSubscriptionReadRepositoryExtensions
{
    [SortExpression<GetWatchedThreadLatestEventPagedQuerySortType>(GetWatchedThreadLatestEventPagedQuerySortType
        .ThreadId)]
    private static readonly Expression<Func<WatchedThreadLatestEvent, ThreadId>> ThreadIdExpression =
        e => e.ThreadId;

    [SortExpression<GetWatchedThreadLatestEventPagedQuerySortType>(GetWatchedThreadLatestEventPagedQuerySortType
        .LatestEvent)]
    private static readonly Expression<Func<WatchedThreadLatestEvent, DateTime>> LatestEventExpression =
        e => e.LatestEvent.OccurredAt;
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

    public async Task<IReadOnlyList<T>> GetLatestEventPerThreadAsync<T>(GetWatchedThreadLatestEventPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable =
            from ts in _dbContext.ThreadSubscriptions.Where(e => e.UserId == query.QueriedBy)
            from ne in _dbContext.NotifiableEvents.Where(e => e.Payload.TestQ(ts.ThreadId))
            select new WatchedThreadLatestEvent
            {
                ThreadId = ts.ThreadId.SqlDistinctOn(ts.ThreadId),
                LatestEvent = ne
            };

        var result = await queryable
            .ApplySort(query)
            .Select(e => e.LatestEvent)
            .ProjectToType<T>()
            .ApplyPagination(query)
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}