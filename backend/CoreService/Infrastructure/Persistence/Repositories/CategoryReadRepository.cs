using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Application.Enums;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Shared.Infrastructure.Persistence.Abstractions;
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

    public async Task<Result<T, CategoryNotFoundError>> GetOneAsync<T>(GetCategoryQuery<T> query,
        CancellationToken cancellationToken) where T : notnull
    {
        var result = await _dbContext.Categories
            .Where(e => e.CategoryId == query.CategoryId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new CategoryNotFoundError(query.CategoryId);

        return result;
    }

    public async Task<Dictionary<CategoryId, Result<T, CategoryNotFoundError>>> GetBulkAsync<T>(
        GetCategoriesBulkQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from p in _dbContext.Categories
                    .Where(e => e.CategoryId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<Guid, Category?>
                {
                    Key = id,
                    Value = p
                })
            .ProjectToType<SqlKeyValue<Guid, T?>>()
            .ToDictionaryAsyncLinqToDB(k => CategoryId.From(k.Key),
                k => (Result<T, CategoryNotFoundError>)(k.Value == null
                    ? new CategoryNotFoundError(CategoryId.From(k.Key))
                    : k.Value), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Categories.AsQueryable();

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

    public async Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesThreadsCountAsync(
        GetCategoriesThreadsCountQuery query, CancellationToken cancellationToken)
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();

        var categoriesCte = (
                from categoryId in _dbContext.ToTvcLinqToDb(ids)
                from c in _dbContext.Categories
                    .Where(e => e.CategoryId == categoryId)
                    .DefaultIfEmpty()
                select new
                {
                    CategoryId = categoryId,
                    IsExists = c != null
                })
            .AsCte();

        var result = await (
                from category in categoriesCte
                from thread in _dbContext.Threads
                    .Where(e => e.CanReadThread(query.QueriedBy) && e.CategoryId == category.CategoryId)
                    .DefaultIfEmpty()
                group thread by category
                into g
                select new { Category = g.Key, ThreadCount = g.CountExt(e => e.ThreadId) })
            .ToDictionaryAsyncLinqToDB(k => CategoryId.From(k.Category.CategoryId),
                v => (Result<Count, CategoryNotFoundError>)(!v.Category.IsExists
                    ? new CategoryNotFoundError(CategoryId.From(v.Category.CategoryId))
                    : Count.From(v.ThreadCount)), cancellationToken);

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
            var q = _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
                .Where(e => e.CategoryId == query.CategoryId)
                .Select(t => new
                {
                    Thread = t,
                    LastPost = _dbContext.Posts.Where(e => e.ThreadId == t.ThreadId)
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
            queryable = _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == query.CategoryId);
        }

        var threads = await queryable
            .Where(e => query.State == null || e.State == query.State)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return threads;
    }

    public async Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesPostsCountAsync(
        GetCategoriesPostsCountQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.CategoryIds.Select(x => x.Value).ToArray();

        var categoriesCte = (
                from categoryId in _dbContext.ToTvcLinqToDb(ids)
                from c in _dbContext.Categories
                    .Where(e => e.CategoryId == categoryId)
                    .DefaultIfEmpty()
                select new
                {
                    CategoryId = categoryId,
                    IsExists = c != null
                })
            .AsCte();

        var result = await (
                from category in categoriesCte
                from thread in _dbContext.Threads
                    .Where(e => e.CanReadThread(query.QueriedBy) && e.CategoryId == category.CategoryId)
                    .DefaultIfEmpty()
                from p in _dbContext.Posts
                    .Where(e => e.ThreadId == thread.ThreadId)
                    .DefaultIfEmpty()
                group p by category
                into g
                select new { Category = g.Key, ThreadCount = g.CountExt(e => e.PostId) })
            .ToDictionaryAsyncLinqToDB(k => CategoryId.From(k.Category.CategoryId),
                v => (Result<Count, CategoryNotFoundError>)(!v.Category.IsExists
                    ? new CategoryNotFoundError(CategoryId.From(v.Category.CategoryId))
                    : Count.From(v.ThreadCount)), cancellationToken);

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
            from t in _dbContext.Threads.Where(e => e.CanReadThread(query.QueriedBy))
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