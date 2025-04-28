using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Application.Enums;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumReadRepository : IForumReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ForumReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(x => x.ForumId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ForumNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(List<ForumId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(x => ids.Contains(x.ForumId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Forum> query = _dbContext.Forums;

        if (request.Title != null)
        {
            query = query.Where(e =>
                e.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        if (request.CreatedBy != null)
        {
            query = query.Where(e => e.CreatedBy == request.CreatedBy.Value);
        }

        if (
            request.Sort != null && request.Sort.Field is GetForumsQuery.SortType.LatestPost
        )
        {
            var subQuery =
                (
                    from f in query
                    from c in _dbContext.Categories.Where(e => e.ForumId == f.ForumId).DefaultIfEmpty()
                    from t in _dbContext.Threads.Where(e => e.CategoryId == c.CategoryId).DefaultIfEmpty()
                    from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId).DefaultIfEmpty()
                    where request.Contains == null || (
                        request.Contains == ForumContainsFilter.Category
                            ? c != null
                            : request.Contains == ForumContainsFilter.Thread
                                ? t != null
                                : p != null
                    )
                    group p by f
                    into g
                    select new
                    {
                        Forum = g.Key,
                        LastPostCreatedAt = Sql.AsSql((DateTime?)g.Max(e => e.CreatedAt) ?? g.Key.CreatedAt)
                    }
                )
                .AsSubQuery();

            query = (
                    request.Sort.Order == SortOrderType.Ascending
                        ? subQuery.OrderBy(e => e.LastPostCreatedAt)
                        : subQuery.OrderByDescending(e => e.LastPostCreatedAt)
                )
                .Select(e => e.Forum);
        }
        else
        {
            query = query
                .OrderBy(e => e.ForumId)
                .AsQueryable();

            if (request.Contains != null)
            {
                switch (request.Contains.Value)
                {
                    case ForumContainsFilter.Category:
                        query = from f in query
                            from c in f.Categories
                            select f;
                        break;
                    case ForumContainsFilter.Thread:
                        query = from f in query
                            from c in f.Categories
                            from t in c.Threads
                            select f;
                        break;
                    case ForumContainsFilter.Post:
                        query = from f in query
                            from c in f.Categories
                            from t in c.Threads
                            from p in t.Posts
                            select f;
                        break;
                }
            }
        }

        var forums = await query
            .ProjectToType<T>()
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<long> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Forums
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy);

        if (request.Contains != null)
        {
            switch (request.Contains.Value)
            {
                case ForumContainsFilter.Category:
                    query = from f in query
                        from c in f.Categories
                        select f;
                    break;
                case ForumContainsFilter.Thread:
                    query = from f in query
                        from c in f.Categories
                        from t in c.Threads
                        select f;
                    break;
                case ForumContainsFilter.Post:
                    query = from f in query
                        from c in f.Categories
                        from t in c.Threads
                        from p in t.Posts
                        select f;
                    break;
            }
        }

        var count = await query
            .Distinct()   
            .LongCountAsyncLinqToDB(cancellationToken);

        return count;
    }

    public async Task<Dictionary<ForumId, long>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken)
    {
        var forums = request.ForumIds.Select(x => x.Value).ToArray();
        var query =
            from f in _dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, forums.ToSqlArray<ForumId>())
            group c by f.ForumId
            into g
            select new { g.Key, Value = g.LongCount() };

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    public async Task<Dictionary<ForumId, T[]>> GetForumsCategoriesLatestAsync<T>(
        GetForumsCategoriesLatestQuery request,
        CancellationToken cancellationToken
    )
        where T : IHasForumId
    {
        var ids = request.ForumIds.Select(x => x.Value).ToArray();
        var latestPostCreatedCte =
            (
                from c in _dbContext.Categories
                from t in _dbContext.Threads.Where(t => t.CategoryId == c.CategoryId).DefaultIfEmpty()
                from p in _dbContext.Posts.Where(p => p.ThreadId == t.ThreadId).DefaultIfEmpty()
                where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.ForumId, ids.ToSqlArray<ForumId>())
                group p by new { c.ForumId, c.CategoryId }
                into g
                select new
                {
                    g.Key.ForumId,
                    g.Key.CategoryId,
                    CreatedAt = g.Max(p => p.CreatedAt)
                }
            )
            .AsCte();

        var rankedCategoriesCte =
            (
                from lpc in latestPostCreatedCte
                select new
                {
                    lpc.CategoryId,
                    lpc.ForumId,
                    Rank = Sql.Ext.RowNumber()
                        .Over()
                        .PartitionBy(lpc.ForumId)
                        .OrderByDesc(lpc.CreatedAt)
                        .ToValue(),
                }
            )
            .AsCte();

        var result = (
                await (
                        from rc in rankedCategoriesCte
                        join c in _dbContext.Categories
                            on rc.CategoryId equals c.CategoryId
                        where rc.Rank <= request.Count
                        orderby rc.ForumId, rc.Rank
                        select c
                    )
                    .ProjectToType<T>()
                    .ToListAsyncLinqToDB(cancellationToken)
            )
            .GroupBy(e => e.ForumId)
            .ToDictionary(g => g.Key, g => g.ToArray());
        return result;
    }
}