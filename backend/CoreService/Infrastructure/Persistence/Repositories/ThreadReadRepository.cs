using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Infrastructure.Extensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadReadRepository : IThreadReadRepository
{
    private readonly ReadonlyApplicationDbContext _dbContext;

    public ThreadReadRepository(ReadonlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ThreadNotFoundError>> GetOneAsync<T>(ThreadId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => x.ThreadId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ThreadNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(HashSet<ThreadId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => ids.Contains(x.ThreadId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery request, CancellationToken cancellationToken)
    {
        var threads = await _dbContext.Threads
            .OrderBy(e => e.ThreadId)
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy)
            .Where(e => request.Status == null || e.Status == request.Status)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }

    public async Task<long> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Threads
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy)
            .Where(e => request.Status == null || e.Status == request.Status)
            .LongCountAsyncLinqToDB(cancellationToken);

        return count;
    }

    public async Task<Dictionary<ThreadId, long>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from t in _dbContext.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids)
            group p by t.ThreadId
            into g
            select new { g.Key, Value = g.LongCount() };

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    public async Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(
        GetThreadsPostsLatestQuery request,
        CancellationToken cancellationToken
    )
        where T : IHasThreadId
    {
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from p in _dbContext.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.ThreadId, ids)
            orderby p.ThreadId, p.PostId descending
            select new
            {
                ThreadId = p.ThreadId.SqlDistinctOn(p.ThreadId),
                p.PostId,
                p.CreatedAt,
                p.CreatedBy,
                p.Content,
                p.UpdatedAt,
                p.UpdatedBy,
                p.RowVersion
            };
        return await query.ProjectToType<T>().ToDictionaryAsyncLinqToDB(k => k.ThreadId, v => v, cancellationToken);
    }

    public async Task<OneOf<long, PostNotFoundError>> GetPostOrderAsync(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Posts
            .Where(x => x.ThreadId == threadId && x.PostId == postId)
            .Select(x => new
            {
                RowNum = _dbContext.Posts.Count(y => y.ThreadId == threadId && (y.PostId < postId)) + 1
            })
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (order == null) return new PostNotFoundError(threadId, postId);

        return order.RowNum;
    }
}