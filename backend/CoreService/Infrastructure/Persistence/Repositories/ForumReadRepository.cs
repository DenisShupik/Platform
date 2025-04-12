using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Extensions;
using SharedKernel.Sorting;

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
        var projection = await AsyncExtensions.ToListAsync(_dbContext.Forums
            .Where(x => ids.Contains(x.ForumId))
            .ProjectToType<T>(), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Forum> query;
        if (request.Sort != null && request.Sort.Field == GetForumsQuery.SortType.LatestPost)
        {
            var q = (
                    from f in _dbContext.Forums
                    join c in _dbContext.Categories on f.ForumId equals c.ForumId into gc
                    from c in gc.DefaultIfEmpty()
                    join t in _dbContext.Threads on c.CategoryId equals t.CategoryId into gt
                    from t in gt.DefaultIfEmpty()
                    join p in _dbContext.Posts on t.ThreadId equals p.ThreadId into gp
                    from p in gp.DefaultIfEmpty()
                    group p by new { f.ForumId, f.Title, f.Created, f.CreatedBy }
                    into g
                    select new
                    {
                        g.Key.ForumId,
                        g.Key.Title,
                        g.Key.Created,
                        g.Key.CreatedBy,
                        LastPostDate = g.Max(p => (DateTime?)p.Created)
                    }
                )
                .AsCte();

            q = request.Sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.LastPostDate.SqlIsNotNull()).ThenBy(e => e.LastPostDate ?? e.Created)
                : q.OrderByDescending(e => e.LastPostDate.SqlIsNotNull())
                    .ThenByDescending(e => e.LastPostDate ?? e.Created);

            query = q.Select(e => new Forum
            {
                ForumId = e.ForumId,
                Title = e.Title,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            });
        }
        else
        {
            query = _dbContext.Forums
                .OrderBy(e => e.ForumId)
                .AsQueryable();
        }

        var a = request.Title?.Value;

        var forums = await query
            .Where(x => a == null || x.Title.VogenToSql().Contains(a, StringComparison.CurrentCultureIgnoreCase))
            .ProjectToType<T>()
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<Dictionary<ForumId, long>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken)
    {
        var forums = request.ForumIds.Select(x => x.Value).ToArray();
        var query =
            from f in _dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, forums.ToSqlGuid<Guid, ForumId>())
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
                from t in c.Threads
                from p in t.Posts
                where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.ForumId, ids.ToSqlGuid<Guid, ForumId>())
                group p by new { c.ForumId, c.CategoryId }
                into g
                select new
                {
                    g.Key.ForumId,
                    g.Key.CategoryId,
                    Created = g.Max(p => p.Created)
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
                        .OrderByDesc(lpc.Created)
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