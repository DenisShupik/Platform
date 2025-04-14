using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SharedKernel.Extensions;
using SharedKernel.Sorting;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, CategoryNotFoundError>> GetOneAsync<T>(CategoryId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(x => x.CategoryId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new CategoryNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(List<CategoryId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(x => ids.Contains(x.CategoryId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories
            .OrderBy(e => e.CategoryId)
            .Where(x => request.ForumId == null || x.ForumId == request.ForumId);

        if (request.Title != null)
        {
            query = query.Where(x =>
                x.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        var result = await query
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }

    public async Task<Dictionary<CategoryId, long>> GetCategoriesThreadsCountAsync(
        GetCategoriesThreadsCountQuery request, CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from c in _dbContext.Categories
            from t in c.Threads
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids.ToSqlArray<CategoryId>())
            group t by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };
        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
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

    public async Task<Dictionary<CategoryId, long>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from c in _dbContext.Categories
            from t in c.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids.ToSqlArray<CategoryId>())
            group p by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    private sealed class GetCategoriesPostsLatestProjection<T>
    {
        // WARN: нельзя менять порядок, необходимо для работы DistinctOn
        public T Post { get; set; }
        public CategoryId CategoryId { get; set; }
    }

    public async Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(GetCategoriesPostsLatestQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from c in _dbContext.Categories
            from t in c.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids.ToSqlArray<CategoryId>())
            select new { c, p };

        var posts = await query
            .OrderBy(e => e.c.CategoryId)
            .ThenByDescending(e => e.p.PostId)
            .Select(e => new
            {
                Post = new
                {
                    // TODO: найти способ автоматически проецировать все поля
                    PostId = e.p.PostId.SqlDistinctOn(e.c.CategoryId),
                    e.p.ThreadId,
                    e.p.Created,
                    e.p.CreatedBy,
                    e.p.Content
                },
                e.c.CategoryId
            })
            .ProjectToType<GetCategoriesPostsLatestProjection<T>>()
            .ToDictionaryAsyncLinqToDB(k => k.CategoryId, v => v.Post, cancellationToken);

        return posts;
    }
}