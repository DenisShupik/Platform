using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Application.Enums;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Generator.Attributes;
using static CoreService.Application.UseCases.GetCategoriesPagedQuery;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

[AddApplySort(typeof(GetCategoriesPagedQuerySortType), typeof(Category))]
internal static partial class CategoryReadRepositoryExtensions
{
    private static readonly Expression<Func<Category, CategoryId>> CategoryIdExpression = e => e.CategoryId;
    private static readonly Expression<Func<Category, ForumId>> ForumIdExpression = e => e.ForumId;
}

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public CategoryReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, CategoryNotFoundError>> GetOneAsync<T>(CategoryId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(e => e.CategoryId == id)
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

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories.AsQueryable();

        if (request.ForumIds != null)
        {
            var ids = request.ForumIds.Select(x => x.Value).ToArray();
            query = query.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.ForumId, ids));
        }

        if (request.Title != null)
        {
            query = query.Where(x =>
                x.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }
        
        var result = await query
            .ApplySort(request)
            .ApplyPagination(request)
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
            from t in c.Threads.Where(e => request.IncludeDraft || e.Status == ThreadStatus.Published)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids)
            group t by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };
        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetCategoryThreadsAsync<T>(GetCategoryThreadsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Thread> query;
        if (request.Sort is { Field: GetCategoryThreadsQuery.GetCategoryThreadsQuerySortType.Activity } sort)
        {
            var q = _dbContext.Threads
                .Where(t => t.CategoryId == request.CategoryId &&
                            (request.IncludeDraft || t.Status == ThreadStatus.Published))
                .Select(t => new
                {
                    Thread = t,
                    LastPost = t.Posts
                        .OrderByDescending(p => p.CreatedAt)
                        .ThenByDescending(p => p.PostId)
                        .Select(p => new { p.CreatedAt, p.PostId })
                        .FirstOrDefault()
                });

            // HINT: можно было бы сделать e.LastPost != null вместо SqlNullLast
            q = sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.LastPost.CreatedAt.SqlNullsLast())
                    .ThenBy(e => new
                    {
                        e.LastPost.PostId,
                        e.Thread.ThreadId
                    })
                : q.OrderBy(e => e.LastPost.CreatedAt.SqlDescNullsLast())
                    .ThenByDescending(e => new
                    {
                        e.LastPost.PostId,
                        e.Thread.ThreadId
                    });

            query = q.Select(e => e.Thread);
        }
        else
        {
            query = _dbContext.Threads
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == request.CategoryId);
        }

        var threads = await query
            .ApplyPagination(request)
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
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids)
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
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids)
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
                    e.p.CreatedAt,
                    e.p.CreatedBy,
                    e.p.Content,
                    e.p.UpdatedAt,
                    e.p.UpdatedBy,
                    e.p.RowVersion
                },
                e.c.CategoryId
            })
            .ProjectToType<GetCategoriesPostsLatestProjection<T>>()
            .ToDictionaryAsyncLinqToDB(k => k.CategoryId, v => v.Post, cancellationToken);

        return posts;
    }
}