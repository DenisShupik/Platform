using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Generator;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Application.Enums;
using Shared.Domain.Extensions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
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
            .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow)
            .Where(e => e.CategoryId == query.CategoryId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new CategoryNotFoundError(query.CategoryId);

        return result;
    }

    public async Task<Result<AuthorizationScope, CategoryNotFoundError>> GetAuthorizationScopeAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var forumId = await _dbContext.Categories
            .Where(category => category.CategoryId == categoryId)
            .Select(category => (ForumId?)category.ForumId)
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        return forumId is null
            ? new CategoryNotFoundError(categoryId)
            : AuthorizationScope.Category(forumId.Value, categoryId);
    }

    public async Task<Dictionary<CategoryId, Result<T, CategoryNotFoundError>>> GetBulkAsync<T>(
        GetCategoriesBulkQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(query.CategoryIds)
                from p in _dbContext.Categories
                    .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow)
                    .Where(e => e.CategoryId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<CategoryId, Category?>
                {
                    Key = id,
                    Value = p
                })
            .ProjectToType<SqlKeyValue<CategoryId, T?>>()
            .ToDictionaryAsyncLinqToDB(k => k.Key,
                k => (Result<T, CategoryNotFoundError>)(k.Value == null
                    ? new CategoryNotFoundError(k.Key)
                    : k.Value), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Categories
            .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow);

        if (query.ForumIds != null)
        {
            queryable = queryable.Where(e => query.ForumIds.Contains(e.ForumId));
        }

        if (query.Title is { } title)
        {
            queryable = queryable.Where(x =>
                x.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
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
        var categoriesCte = (
                from categoryId in _dbContext.ToTvcLinqToDb(query.CategoryIds)
                from c in _dbContext.Categories
                    .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow)
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
                    .Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow) && e.CategoryId == category.CategoryId)
                    .DefaultIfEmpty()
                group thread by category
                into g
                select new { Category = g.Key, ThreadCount = g.CountExt(e => e.ThreadId) })
            .ToDictionaryAsyncLinqToDB(k => k.Category.CategoryId,
                v => (Result<Count, CategoryNotFoundError>)(!v.Category.IsExists
                    ? new CategoryNotFoundError(v.Category.CategoryId)
                    : Count.From(v.ThreadCount)), cancellationToken);

        return result;
    }

    public async Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> GetCategoryThreadsAsync<T>(
        GetCategoryThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        IQueryable<Thread> queryable;
        if (query.Sort is { Field: GetCategoryThreadsPagedQuerySortType.Activity } sort)
        {
            var q = _dbContext.Threads.Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow))
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

            q = sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.LastPost!.CreatedAt, Sql.NullsPosition.Last)
                    .ThenBy(e => new
                    {
                        e.LastPost!.PostId,
                        e.Thread.ThreadId
                    })
                : q.OrderByDescending(e => e.LastPost!.CreatedAt, Sql.NullsPosition.Last)
                    .ThenByDescending(e => new
                    {
                        e.LastPost!.PostId,
                        e.Thread.ThreadId
                    });

            queryable = q.Select(e => e.Thread);
        }
        else
        {
            queryable = _dbContext.Threads.Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow))
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == query.CategoryId);
        }

        queryable = queryable
            .Where(e => query.State == null || e.State == query.State)
            .ApplyPagination(query);

        var projections = await (
                from category in _dbContext.Categories
                    .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow)
                    .Where(e => e.CategoryId == query.CategoryId)
                from thread in queryable.DefaultIfEmpty()
                select new SqlKeyValue<CategoryId, Thread?>
                {
                    Key = category.CategoryId,
                    Value = thread
                })
            .ProjectToType<SqlKeyValue<CategoryId, T?>>()
            .ToListAsyncLinqToDB(cancellationToken);

        if (projections.Count == 0) return new CategoryNotFoundError(query.CategoryId);

        return projections
            .Where(e => e.Value is not null)
            .Select(e => e.Value!)
            .ToList();
    }

    public async Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesPostsCountAsync(
        GetCategoriesPostsCountQuery query,
        CancellationToken cancellationToken)
    {
        var categoriesCte = (
                from categoryId in _dbContext.ToTvcLinqToDb(query.CategoryIds)
                from c in _dbContext.Categories
                    .WhereCanRead(_dbContext, query.QueriedBy, DateTime.UtcNow)
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
                    .Where(e => _dbContext.CanReadThread(e, query.QueriedBy, DateTime.UtcNow) && e.CategoryId == category.CategoryId)
                    .DefaultIfEmpty()
                group thread by category
                into g
                select new
                {
                    Category = g.Key,
                    PostCount = g.Sum(thread => thread == null ? 0 : (int)thread.PostCount)
                })
            .ToDictionaryAsyncLinqToDB(k => k.Category.CategoryId,
                v => (Result<Count, CategoryNotFoundError>)(!v.Category.IsExists
                    ? new CategoryNotFoundError(v.Category.CategoryId)
                    : Count.From(v.PostCount)), cancellationToken);

        return result;
    }

    public async Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(
        GetCategoriesPostsLatestQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable =
            from categoryId in _dbContext.ToTvcLinqToDb(query.CategoryIds)
            from post in (
                    from thread in _dbContext.Threads
                    where thread.CategoryId == categoryId && _dbContext.CanReadThread(thread, query.QueriedBy, DateTime.UtcNow)
                    from candidate in _dbContext.Posts.Where(e => e.ThreadId == thread.ThreadId)
                    orderby candidate.CreatedAt descending, candidate.PostId descending
                    select candidate)
                .Take(1)
            select new SqlKeyValue<CategoryId, Post>
            {
                Key = categoryId,
                Value = post
            };

        var posts = await queryable
            .ProjectToType<SqlKeyValue<CategoryId, T>>()
            .ToDictionaryAsyncLinqToDB(k => k.Key, v => v.Value, cancellationToken);

        return posts;
    }
}
