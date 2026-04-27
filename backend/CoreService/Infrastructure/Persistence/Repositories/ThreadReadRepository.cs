using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Shared.Infrastructure.Persistence.Abstractions;
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


    public async Task<Result<T, ThreadNotFoundError, PermissionDeniedError>> GetOneAsync<T>(
        GetThreadQuery<T> query,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await _dbContext.Threads
            .Where(e => e.ThreadId == query.ThreadId)
            .Select(e => new ProjectionWithAccess<Thread>
            {
                Projection = e,
                HasAccess = e.CanReadThread(query.QueriedBy)
            })
            .ProjectToType<ProjectionWithAccess<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ThreadNotFoundError();
        if (!result.HasAccess) return new PermissionDeniedError();

        return result.Projection;
    }


    public async Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError>>> GetBulkAsync<T>(
        GetThreadsBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull
    {
        var ids = query.ThreadIds.Select(x => x.Value).ToArray();
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<ThreadId, ProjectionWithAccess<Thread>?>
                {
                    Key = ThreadId.From(id),
                    Value = t == null
                        ? null
                        : new ProjectionWithAccess<Thread>
                        {
                            Projection = t,
                            HasAccess = t.CanReadThread(query.QueriedBy)
                        }
                })
            .ProjectToType<SqlKeyValue<ThreadId, ProjectionWithAccess<T>?>>()
            .ToDictionaryAsyncLinqToDB(k => k.Key,
                v => (Result<T, ThreadNotFoundError, PermissionDeniedError>)(
                    v.Value == null
                        ? new ThreadNotFoundError()
                        : !v.Value.HasAccess
                            ? new PermissionDeniedError()
                            : v.Value.Projection
                )
                , cancellationToken);

        return projection;
    }

    public async Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> query, CancellationToken cancellationToken)
    {
        var threads = await _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);
        return threads;
    }

    public async Task<Count> GetCountAsync(GetThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
            .CountAsyncLinqToDB(cancellationToken);

        return Count.From(count);
    }

    public async Task<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>
        GetThreadsPostsCountAsync(GetThreadsPostsCountQuery query,
            CancellationToken cancellationToken)
    {
        var ids = query.ThreadIds.Select(x => x.Value).ToArray();

        var availableThreads = (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new
                {
                    ThreadId = id,
                    CanRead = t != null ? t.CanReadThread(query.QueriedBy) : (bool?)null
                }
            )
            .AsCte();

        var result = await (
                    from at in availableThreads
                    from p in _dbContext.Posts
                        .Where(e => e.ThreadId == at.ThreadId && at.CanRead != null && at.CanRead.Value)
                        .DefaultIfEmpty()
                    group p by at
                    into g
                    select new { g.Key, Value = g.CountExt(e => e.PostId) })
                .ToDictionaryAsyncLinqToDB(k => ThreadId.From(k.Key.ThreadId),
                    v => (Result<Count, ThreadNotFoundError, PermissionDeniedError>)(v.Key.CanRead == null
                        ? new ThreadNotFoundError()
                        : !v.Key.CanRead.Value
                            ? new PermissionDeniedError()
                            : Count.From(v.Value)), cancellationToken)
            ;

        return result;
    }

    private sealed class GetThreadsPostsLatestProjection<T>
    {
        public ThreadId ThreadId { get; set; }
        public bool? CanRead { get; set; }
        public T? Post { get; set; }
    }

    public async Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
        GetThreadsPostsLatestAsync<T>(
            GetThreadsPostsLatestQuery<T> query,
            CancellationToken cancellationToken
        )
        where T : IHasThreadId
    {
        var ids = query.ThreadIds.Select(x => x.Value).ToArray();

        var availableThreads = (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new
                {
                    ThreadId = id,
                    CanRead = t != null ? t.CanReadThread(query.QueriedBy) : (bool?)null
                }
            )
            .AsCte();

        var result = await (
                    from at in availableThreads
                    from p in _dbContext.Posts
                        .Where(e => e.ThreadId == at.ThreadId && at.CanRead != null && at.CanRead.Value)
                        .DefaultIfEmpty()
                    select new { at.ThreadId, at.CanRead, Post = p })
                .OrderBy(e => e.ThreadId)
                .ThenByDescending(e => e.Post.CreatedAt)
                .ThenByDescending(e => e.Post.PostId)
                .Select(e => new
                {
                    ThreadId = e.ThreadId.SqlDistinctOn(e.ThreadId),
                    e.CanRead,
                    e.Post
                })
                .ProjectToType<GetThreadsPostsLatestProjection<T>>()
                .ToDictionaryAsyncLinqToDB(k => k.ThreadId,
                    v => (Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>)(v.CanRead == null
                        ? new ThreadNotFoundError()
                        : !v.CanRead.Value
                            ? new PermissionDeniedError()
                            : v.Post == null
                                ? new PostNotFoundError()
                                : v.Post), cancellationToken)
            ;

        // var queryable =
        //     from t in _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
        //     from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
        //     where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids)
        //     select new { t.ThreadId, Post = p };
        //
        // var posts = await queryable
        //     .OrderBy(e => e.ThreadId)
        //     .ThenByDescending(e => e.Post.CreatedAt)
        //     .ThenByDescending(e => e.Post.PostId)
        //     .Select(e => new
        //     {
        //         // TODO: найти способ автоматически проецировать все поля
        //         PostId = e.Post.PostId.SqlDistinctOn(e.ThreadId),
        //         e.Post.ThreadId,
        //         e.Post.CreatedAt,
        //         e.Post.CreatedBy,
        //         e.Post.Content,
        //         e.Post.UpdatedAt,
        //         e.Post.UpdatedBy,
        //         e.Post.RowVersion
        //     })
        //     .ProjectToType<T>()
        //     .ToDictionaryAsyncLinqToDB(k => k.ThreadId, v => v, cancellationToken);

        return result;
    }
}