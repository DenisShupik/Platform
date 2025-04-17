using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Infrastructure.Extensions;


namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadReadRepository : IThreadReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ThreadReadRepository(ApplicationDbContext dbContext)
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

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(List<ThreadId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => ids.Contains(x.ThreadId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }

    public async Task<Dictionary<ThreadId, long>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from t in _dbContext.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids.ToSqlArray<ThreadId>())
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
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.ThreadId, ids.ToSqlArray<ThreadId>())
            orderby p.ThreadId, p.PostId descending
            select new
            {
                PostId = p.PostId.SqlDistinctOn(p.ThreadId),
                ThreadId = p.ThreadId,
                Content = p.Content,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
            };
        return await query.ProjectToType<T>().ToDictionaryAsyncLinqToDB(k => k.ThreadId, v => v, cancellationToken);
    }
}