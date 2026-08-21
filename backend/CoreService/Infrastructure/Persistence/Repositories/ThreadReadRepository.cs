using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Generator;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
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

    public async Task<Result<AuthorizationScope, ThreadNotFoundError>> GetAuthorizationScopeAsync(
        ThreadId threadId,
        CancellationToken cancellationToken)
    {
        var scope = await (
                from thread in _dbContext.Threads
                from category in _dbContext.Categories.Where(category => category.CategoryId == thread.CategoryId)
                where thread.ThreadId == threadId
                select new { category.ForumId, thread.CategoryId, thread.ThreadId })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        return scope is null
            ? new ThreadNotFoundError()
            : AuthorizationScope.Thread(scope.ForumId, scope.CategoryId, scope.ThreadId);
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
                HasAccess = _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow)
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
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(query.ThreadIds)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<ThreadId, ProjectionWithAccess<Thread>?>
                {
                    Key = id,
                    Value = t == null
                        ? null
                        : new ProjectionWithAccess<Thread>
                        {
                            Projection = t,
                            HasAccess = _dbContext.CanReadThread(t, query.QueriedBy, DateTime.UtcNow)
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
        var threads = await _dbContext.Threads
            .Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow))
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);
        return threads;
    }

    public async Task<Count> GetCountAsync(GetThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Threads
            .Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow))
            .CountAsyncLinqToDB(cancellationToken);

        return Count.From(count);
    }

    public async Task<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>
        GetThreadsPostsCountAsync(GetThreadsPostsCountQuery query,
            CancellationToken cancellationToken)
    {
        var availableThreads = (
                from id in _dbContext.ToTvcLinqToDb(query.ThreadIds)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new
                {
                    ThreadId = id,
                    CanRead = t != null ? _dbContext.CanReadThread(t, query.QueriedBy, DateTime.UtcNow) : (bool?)null,
                    PostCount = t != null ? t.PostCount : (Count?)null
                }
            )
            .AsCte();

        var result = await availableThreads
                .ToDictionaryAsyncLinqToDB(k => k.ThreadId,
                    v => (Result<Count, ThreadNotFoundError, PermissionDeniedError>)(v.CanRead == null
                        ? new ThreadNotFoundError()
                        : !v.CanRead.Value
                            ? new PermissionDeniedError()
                            : v.PostCount.GetValueOrDefault()), cancellationToken)
            ;

        return result;
    }

    public async Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
        GetThreadsPostsLatestAsync<T>(
            GetThreadsPostsLatestQuery<T> query,
            CancellationToken cancellationToken
        )
        where T : IHasThreadId
    {
        var availableThreads = (
                from id in _dbContext.ToTvcLinqToDb(query.ThreadIds)
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == id)
                    .DefaultIfEmpty()
                select new
                {
                    ThreadId = id,
                    CanRead = t != null ? _dbContext.CanReadThread(t, query.QueriedBy, DateTime.UtcNow) : (bool?)null
                }
            )
            .AsCte();

        var result = await (
                from at in availableThreads
                from p in _dbContext.Posts
                    .Where(e => e.ThreadId == at.ThreadId && at.CanRead != null && at.CanRead.Value)
                    .OrderByDescending(e => e.CreatedAt)
                    .ThenByDescending(e => e.PostId)
                    .Take(1)
                    .DefaultIfEmpty()
                select new SqlKeyValue<ThreadId, SqlKeyValue<bool?, Post?>>
                {
                    Key = at.ThreadId,
                    Value = new SqlKeyValue<bool?, Post?>
                    {
                        Key = at.CanRead,
                        Value = p
                    }
                })
                .ProjectToType<SqlKeyValue<ThreadId, SqlKeyValue<bool?, T?>>>()
                .ToDictionaryAsyncLinqToDB(k => k.Key,
                    v => (Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>)(v.Value.Key == null
                        ? new ThreadNotFoundError()
                        : !v.Value.Key.Value
                            ? new PermissionDeniedError()
                            : v.Value.Value == null
                                ? new PostNotFoundError()
                                : v.Value.Value), cancellationToken);

        return result;
    }
}
