using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using SharedKernel.Extensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = await _dbContext.Posts
            .OrderBy(e => e.PostId)
            .Where(x => request.ThreadId == null || x.ThreadId == request.ThreadId)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return query;
    }

    private sealed class GetCategoriesLatestPostProjection<T>
    {
        // WARN: нельзя менять порядок, необходимо для работы DistinctOn
        public T Post { get; set; }
        public CategoryId CategoryId { get; set; }
    }

    public async Task<Dictionary<CategoryId, T>> GetCategoriesLatestPostAsync<T>(GetCategoriesLatestPostQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from c in _dbContext.Categories
            from t in c.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids.ToSqlGuid<Guid, CategoryId>())
            select new { c, p };

        var posts = await query
            .OrderBy(e => e.c.CategoryId)
            .ThenByDescending(e => e.p.PostId)
            .Select(e => new
            {
                e.c.CategoryId,
                Post = new
                {
                    PostId = e.p.PostId.SqlDistinctOn(e.c.CategoryId),
                    ThreadId = e.p.ThreadId,
                    Created = e.p.Created,
                    CreatedBy = e.p.CreatedBy,
                    Content = e.p.Content
                }
            })
            .ProjectToType<GetCategoriesLatestPostProjection<T>>()
            .ToDictionaryAsyncLinqToDB(k => k.CategoryId, v => v.Post, cancellationToken);

        return posts;
    }
}