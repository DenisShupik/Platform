using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Sorting;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadReadRepository : IThreadReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ThreadReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ThreadNotFoundError>> GetByIdAsync<T>(ThreadId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => x.ThreadId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ThreadNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<ThreadId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Threads
            .Where(x => ids.Contains(x.ThreadId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetCategoryThreadsAsync<T>(GetCategoryThreadsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Thread> query;
        if (request.Sort?.Field == GetCategoryThreadsQuery.GetCategoryThreadsRequestSortType.Activity)
        {
            var latestPosts =
                from t in _dbContext.Threads
                from p in t.Posts
                where t.CategoryId == request.CategoryId
                group p by t.ThreadId
                into g
                select new { ThreadId = g.Key, PostId = g.Max(p => p.PostId) };

            var q =
                from lp in latestPosts
                join t in _dbContext.Threads on lp.ThreadId equals t.ThreadId
                join p in _dbContext.Posts
                    on new { lp.ThreadId, lp.PostId }
                    equals new { p.ThreadId, p.PostId }
                    into g
                from p in g.DefaultIfEmpty()
                select new { t, p };

            q = request.Sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.p.Created)
                : q.OrderByDescending(e => e.p.Created);

            query = q.Select(e => e.t);
        }
        else
        {
            query = _dbContext.Threads
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == request.CategoryId);
        }


        var threads = await query
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }
}