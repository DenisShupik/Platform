using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Application.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetCategoriesPagedQuery<>), typeof(Category))]
internal static partial class CategoryReadRepositoryExtensions;

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public CategoryReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<T, CategoryNotFoundError, PolicyViolationError, AccessPolicyRestrictedError>>
        GetOneAsync<T>(GetCategoryQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await _dbContext.GetCategoriesWithAccessInfo(query.QueriedBy)
            .Where(e => e.Projection.CategoryId == query.CategoryId)
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new CategoryNotFoundError(query.CategoryId);

        if ((result.AccessPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.AccessPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.AccessPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new AccessPolicyRestrictedError(query.QueriedBy);

        return result.Projection;
    }
    
    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<CategoryId, Guid> ids,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(x => ids.ToHashSet().Contains(x.CategoryId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.GetCategoriesWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection);

        if (query.ForumIds != null)
        {
            var ids = query.ForumIds.Select(e => e.Value).ToArray();
            queryable = queryable.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.ForumId, ids));
        }

        if (query.Title != null)
        {
            queryable = queryable.Where(x =>
                x.Title.ToSqlString().Contains(query.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        var result = await queryable
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }

    public async Task<Dictionary<CategoryId, ulong>> GetCategoriesThreadsCountAsync(
        GetCategoriesThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();

        var queryable = _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection)
            .Where(e => (query.IncludeDraft || e.Status == ThreadStatus.Published) &&
                        Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.CategoryId, ids))
            .GroupBy(e => e.CategoryId)
            .Select(e => new { e.Key, Value = e.LongCount() });

        var result = await queryable.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);

        return result;
    }

    public async Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> GetCategoryThreadsAsync<T>(
        GetCategoryThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Categories.AnyAsyncLinqToDB(e => e.CategoryId == query.CategoryId, cancellationToken))
            return new CategoryNotFoundError(query.CategoryId);

        IQueryable<Thread> queryable;
        if (query.Sort is { Field: GetCategoryThreadsPagedQuerySortType.Activity } sort)
        {
            var q = _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                .OnlyAvailable(query.QueriedBy)
                .Select(e => e.Projection)
                .Where(e => e.CategoryId == query.CategoryId &&
                            (query.IncludeDraft || e.Status == ThreadStatus.Published))
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

            queryable = q.Select(e => e.Thread);
        }
        else
        {
            queryable = _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                .OnlyAvailable(query.QueriedBy)
                .Select(e => e.Projection)
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == query.CategoryId);
        }

        var threads = await queryable
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }

    public async Task<Dictionary<CategoryId, ulong>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();

        var query =
            from t in _dbContext.GetThreadsWithAccessInfo(request.QueriedBy)
                .OnlyAvailable(request.QueriedBy)
                .Select(e => e.Projection)
            from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.CategoryId, ids)
            group p by t.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

        var result = await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);

        return result;
    }

    private sealed class GetCategoriesPostsLatestProjection<T>
    {
        // WARN: нельзя менять порядок, необходимо для работы DistinctOn
        public T Post { get; set; }
        public CategoryId CategoryId { get; set; }
    }

    public async Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(
        GetCategoriesPostsLatestQuery<T> query,
        CancellationToken cancellationToken)
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();
        
        var queryable =
            from t in _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                .OnlyAvailable(query.QueriedBy)
                .Select(e => e.Projection)
            from p in _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.CategoryId, ids)
            select new { t.CategoryId, Post = p };

        var posts = await queryable
            .OrderBy(e => e.CategoryId)
            .ThenByDescending(e => e.Post.CreatedAt)
            .ThenByDescending(e => e.Post.PostId)
            .Select(e => new
            {
                Post = new
                {
                    // TODO: найти способ автоматически проецировать все поля
                    PostId = e.Post.PostId.SqlDistinctOn(e.CategoryId),
                    e.Post.ThreadId,
                    e.Post.CreatedAt,
                    e.Post.CreatedBy,
                    e.Post.Content,
                    e.Post.UpdatedAt,
                    e.Post.UpdatedBy,
                    e.Post.RowVersion
                },
                e.CategoryId
            })
            .ProjectToType<GetCategoriesPostsLatestProjection<T>>()
            .ToDictionaryAsyncLinqToDB(k => k.CategoryId, v => v.Post, cancellationToken);

        return posts;
    }
}