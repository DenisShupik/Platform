using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetActivitiesPagedQuery<>), typeof(PostAddedActivity))]
internal static partial class ActivityReadRepositoryExtensions
{
    [SortExpression<GetActivitiesPagedQuerySortType>(GetActivitiesPagedQuerySortType.Latest)]
    private static readonly Expression<Func<PostAddedActivity, object>> LatestExpression =
        e => new { e.OccurredAt, e.PostId };
}

public sealed class ActivityReadRepository : IActivityReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ActivityReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetActivitiesPagedQuery<T> request,
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
                        .PartitionBy(request.GetActivitiesPagedQueryGroupBy ==
                                     GetActivitiesPagedQueryGroupByType.Forum
                            ? f.ForumId
                            : request.GetActivitiesPagedQueryGroupBy ==
                              GetActivitiesPagedQueryGroupByType.Category
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

        var result = await query
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }
}