using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using SharedKernel.Application.Enums;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ActivityReadRepository : IActivityReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ActivityReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetActivitiesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from a in
                from f in _dbContext.Forums
                from c in _dbContext.Categories.Where(e => e.ForumId == f.ForumId)
                from t in _dbContext.Threads.Where(e => e.CategoryId == c.CategoryId)
                from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
                select new
                {
                    f.ForumId,
                    c.CategoryId,
                    t.ThreadId,
                    p.PostId,
                    OccurredBy = p.CreatedBy,
                    OccurredAt = p.CreatedAt,
                    Rank = Sql.Ext.RowNumber().Over()
                        .PartitionBy(request.GetActivitiesPagedQueryGroupBy == GetActivitiesPagedQuery.GetActivitiesPagedQueryGroupByType.Forum
                            ? f.ForumId
                            : request.GetActivitiesPagedQueryGroupBy == GetActivitiesPagedQuery.GetActivitiesPagedQueryGroupByType.Category
                                ? c.CategoryId
                                : t.ThreadId)
                        .OrderByDesc(p.CreatedAt)
                        .ThenByDesc(p.PostId)
                        .ToValue()
                }
            where a.Rank == 1
            select new PostAddedActivity
            {
                ForumId = a.ForumId,
                CategoryId = a.CategoryId,
                ThreadId = a.ThreadId,
                PostId = a.PostId,
                OccurredBy = a.OccurredBy,
                OccurredAt = a.OccurredAt
            };
        
        if (request.Sort is { Field: GetActivitiesPagedQuery.GetActivitiesPagedQuerySortType.Latest } sort)
        {
            query = sort.Order == SortOrderType.Ascending
                ? query.OrderBy(e => new { e.OccurredAt, e.PostId })
                : query.OrderByDescending(e => new { e.OccurredAt, e.PostId });
        }
        
        var result = await query.ProjectToType<T>().ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}