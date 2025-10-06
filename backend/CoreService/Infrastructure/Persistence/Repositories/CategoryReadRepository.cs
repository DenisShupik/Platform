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
using Shared.Application.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using UserService.Domain.ValueObjects;
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

    private IQueryable<Category> GetCategories(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<Category> queryable;
        if (userId == null)
        {
            queryable =
                from c in _dbContext.Categories
                from ap in _dbContext.Policies.Where(e => e.PolicyId == c.AccessPolicyId && e.Value == PolicyValue.Any)
                select c;
        }
        else
        {
            queryable =
                from c in _dbContext.Categories
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == c.AccessPolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.UserId == userId && e.PolicyId == c.AccessPolicyId)
                    .DefaultIfEmpty()
                from fr in _dbContext.ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from cr in _dbContext.CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                where cr == null && fr == null && (ap.Value < PolicyValue.Granted || ag.PolicyId.SqlIsNotNull())
                select c;
        }

        return queryable;
    }

    private IQueryable<Thread> GetThreads(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<Thread> queryable;
        if (userId == null)
        {
            queryable =
                from t in _dbContext.Threads
                from ap in _dbContext.Policies.Where(e => e.PolicyId == t.AccessPolicyId && e.Value == PolicyValue.Any)
                select t;
        }
        else
        {
            queryable =
                from t in _dbContext.Threads
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == t.AccessPolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.UserId == userId && e.PolicyId == t.AccessPolicyId)
                    .DefaultIfEmpty()
                from fr in _dbContext.ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from cr in _dbContext.CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from tr in _dbContext.ThreadRestrictions
                    .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                where tr == null && cr == null && fr == null &&
                      (ap.Value < PolicyValue.Granted || ag.PolicyId.SqlIsNotNull())
                select t;
        }

        return queryable;
    }

    private sealed class PostThread
    {
        public Thread Thread { get; set; }
        public Post Post { get; set; }
    }

    private IQueryable<PostThread> GetPosts(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<PostThread> queryable;
        if (userId == null)
        {
            queryable =
                from p in _dbContext.Posts
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == t.AccessPolicyId && e.Value == PolicyValue.Any)
                select new PostThread { Thread = t, Post = p };
        }
        else
        {
            queryable =
                from p in _dbContext.Posts
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == t.AccessPolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.UserId == userId && e.PolicyId == t.AccessPolicyId)
                    .DefaultIfEmpty()
                from fr in _dbContext.ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from cr in _dbContext.CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from tr in _dbContext.ThreadRestrictions
                    .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                where tr == null && cr == null && fr == null &&
                      (ap.Value < PolicyValue.Granted || ag.PolicyId.SqlIsNotNull())
                select new PostThread { Thread = t, Post = p };
        }

        return queryable;
    }

    public async Task<Result<T, CategoryNotFoundError>> GetOneAsync<T>( GetCategoryQuery<T> query,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var projection = await GetCategories(query.QueriedBy)
            .Where(e => e.CategoryId == query.CategoryId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new CategoryNotFoundError(query.CategoryId);
        
        return projection;
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

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        var queryable = GetCategories(request.QueriedBy);

        if (request.ForumIds != null)
        {
            var ids = request.ForumIds.Select(e => e.Value).ToArray();
            queryable = queryable.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.ForumId, ids));
        }

        if (request.Title != null)
        {
            queryable = queryable.Where(x =>
                x.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        var result = await queryable
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return result;
    }

    public async Task<Dictionary<CategoryId, ulong>> GetCategoriesThreadsCountAsync(
        GetCategoriesThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();
        var queryable =
            from t in GetThreads(query.QueriedBy)
            where (query.IncludeDraft || t.Status == ThreadStatus.Published) &&
                  Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.CategoryId, ids)
            group t by t.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

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
            var q = GetThreads(query.QueriedBy)
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
            queryable = GetThreads(query.QueriedBy)
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
            from p in GetPosts(request.QueriedBy)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.Thread.CategoryId, ids)
            group p by p.Thread.CategoryId
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
        GetCategoriesPostsLatestQuery<T> request,
        CancellationToken cancellationToken)
    {
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from p in GetPosts(request.QueriedBy)
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.Thread.CategoryId, ids)
            select p;

        var posts = await query
            .OrderBy(e => e.Thread.CategoryId)
            .ThenByDescending(e => e.Post.PostId)
            .Select(e => new
            {
                Post = new
                {
                    // TODO: найти способ автоматически проецировать все поля
                    PostId = e.Post.PostId.SqlDistinctOn(e.Thread.CategoryId),
                    e.Post.ThreadId,
                    e.Post.CreatedAt,
                    e.Post.CreatedBy,
                    e.Post.Content,
                    e.Post.UpdatedAt,
                    e.Post.UpdatedBy,
                    e.Post.RowVersion
                },
                e.Thread.CategoryId
            })
            .ProjectToType<GetCategoriesPostsLatestProjection<T>>()
            .ToDictionaryAsyncLinqToDB(k => k.CategoryId, v => v.Post, cancellationToken);

        return posts;
    }
}