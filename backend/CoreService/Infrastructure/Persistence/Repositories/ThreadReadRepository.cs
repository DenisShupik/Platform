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
using Shared.Domain.Abstractions;
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

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ThreadId, Guid> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => ids.ToHashSet().Contains(x.ThreadId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> request, CancellationToken cancellationToken)
    {
        var threads = await _dbContext.Threads
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy)
            .Where(e => request.Status == null || e.Status == request.Status)
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }

    public async Task<ulong> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Threads
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy)
            .Where(e => request.Status == null || e.Status == request.Status)
            .LongCountAsyncLinqToDB(cancellationToken);

        return (ulong)count;
    }

    public async Task<Dictionary<ThreadId, ulong>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
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

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);
    }

    public async Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(
        GetThreadsPostsLatestQuery<T> request,
        CancellationToken cancellationToken
    )
        where T : IHasThreadId
    {
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from p in _dbContext.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.ThreadId, ids)
            orderby p.ThreadId, p.CreatedAt descending, p.PostId descending
            select new
            {
                PostId = p.PostId.SqlDistinctOn(p.ThreadId),
                p.ThreadId,
                p.CreatedAt,
                p.CreatedBy,
                p.Content,
                p.UpdatedAt,
                p.UpdatedBy,
                p.RowVersion
            };
        return await query.ProjectToType<T>().ToDictionaryAsyncLinqToDB(k => k.ThreadId, v => v, cancellationToken);
    }

    public async Task<OneOf<PostIndex, PostNotFoundError>> GetPostIndexAsync(PostId postId,
        CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .Where(e => e.PostId == postId)
            .Select(e => new
            {
                Index = _dbContext.Posts.LongCount(p =>
                    p.ThreadId == e.ThreadId && Sql.Row(p.CreatedAt, p.PostId) < Sql.Row(e.CreatedAt, e.PostId))
            })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (post == null) return new PostNotFoundError(postId);

        return PostIndex.From((ulong)post.Index);
    }
}