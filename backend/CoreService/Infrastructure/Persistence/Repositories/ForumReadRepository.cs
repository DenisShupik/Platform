using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
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

    public async Task<OneOf<T, ForumNotFoundError>> GetByIdAsync<T>(ForumId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(x => x.ForumId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ForumNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<ForumId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(x => ids.Contains(x.ForumId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

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
        
        var forums = await query
            .ProjectToType<T>()
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }
}