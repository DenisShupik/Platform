using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetThreadsPagedQuery<>), typeof(Thread))]
internal static partial class ThreadReadRepositoryExtensions;

public sealed class ThreadReadRepository : IThreadReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ThreadReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, ThreadNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>> GetOneAsync<T>(
        GetThreadQuery<T> query,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
            .Where(e => e.Projection.ThreadId == query.ThreadId)
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ThreadNotFoundError(query.ThreadId);

        if ((result.ReadPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.ReadPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.ReadPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new ReadPolicyRestrictedError(query.QueriedBy);

        return result.Projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ThreadId, Guid> ids, CancellationToken cancellationToken)
    {
        // TODO: переделать на получение словаря с Result
        var projection = await _dbContext.Threads
            .Where(x => ids.ToHashSet().Contains(x.ThreadId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> query, CancellationToken cancellationToken)
    {
        var threads = await _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection)
            .Where(e => query.CreatedBy == null || e.CreatedBy == query.CreatedBy)
            .Where(e => query.Status == null || e.Status == query.Status)
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }

    public async Task<ulong> GetCountAsync(GetThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var count = await _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection)
            .Where(e => query.CreatedBy == null || e.CreatedBy == query.CreatedBy)
            .Where(e => query.Status == null || e.Status == query.Status)
            .LongCountAsyncLinqToDB(cancellationToken);

        return (ulong)count;
    }

    public async Task<Dictionary<ThreadId, ulong>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.ThreadIds.Select(x => x.Value).ToArray();

        var queryable =
            from t in _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                .OnlyAvailable(query.QueriedBy)
                .Select(e => e.Projection)
            from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids)
            group p by t.ThreadId
            into g
            select new { g.Key, Value = g.LongCount() };

        var result = await queryable.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);

        return result;
    }

    public async Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(
        GetThreadsPostsLatestQuery<T> query,
        CancellationToken cancellationToken
    )
        where T : IHasThreadId
    { 
        var ids = query.ThreadIds.Select(x => x.Value).ToArray();

        var queryable =
            from t in _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                .OnlyAvailable(query.QueriedBy)
                .Select(e => e.Projection)
            from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids)
            select new { t.ThreadId, Post = p };

        var posts = await queryable
            .OrderBy(e => e.ThreadId)
            .ThenByDescending(e => e.Post.CreatedAt)
            .ThenByDescending(e => e.Post.PostId)
            .Select(e => new
            {
                // TODO: найти способ автоматически проецировать все поля
                PostId = e.Post.PostId.SqlDistinctOn(e.ThreadId),
                e.Post.ThreadId,
                e.Post.CreatedAt,
                e.Post.CreatedBy,
                e.Post.Content,
                e.Post.UpdatedAt,
                e.Post.UpdatedBy,
                e.Post.RowVersion
            })
            .ProjectToType<T>()
            .ToDictionaryAsyncLinqToDB(k => k.ThreadId, v => v, cancellationToken);

        return posts;
    }
}