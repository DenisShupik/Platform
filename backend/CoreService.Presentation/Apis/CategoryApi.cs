using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Paging;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Application.DTOs;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using SharedKernel.Sorting;
using Thread = CoreService.Domain.Entities.Thread;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("{categoryIds}/posts/count", GetCategoryPostsCountAsync).AllowAnonymous();
        api.MapGet("{categoryIds}/posts", GetCategoryPostsAsync).AllowAnonymous();
        api.MapGet("{categoryId}", GetCategoryAsync).AllowAnonymous();
        api.MapGet("{categoryIds}/threads/count", GetCategoryThreadsCountAsync).AllowAnonymous();
        api.MapGet("{categoryId}/threads", GetCategoryThreadsAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateCategoryAsync);

        return app;
    }

    private static async Task<Ok<Dictionary<long, long>>> GetCategoryPostsCountAsync(
        [AsParameters] GetCategoryPostsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from c in dbContext.Categories
            from t in c.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, request.CategoryIds.ToArray())
            group p by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Ok<List<GetCategoryPostsResponse>>> GetCategoryPostsAsync(
        [AsParameters] GetCategoryPostsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from c in dbContext.Categories
            from t in c.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, request.CategoryIds.ToArray())
            select new { c, t, p };


        var posts = query
            .OrderBy(e => e.c.CategoryId)
            .ThenByDescending(e => e.p.PostId)
            .Select(e => new GetCategoryPostsResponse
            {
                Post = new Post
                {
                    PostId = request.Latest ? e.p.PostId.SqlDistinctOn(e.c.CategoryId) : e.p.PostId,
                    ThreadId = e.p.ThreadId,
                    Created = e.p.Created,
                    CreatedBy = e.p.CreatedBy,
                    Content = e.p.Content
                },
                CategoryId = e.c.CategoryId,
            });


        return TypedResults.Ok(await posts.ToListAsyncLinqToDB(cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<Category>>> GetCategoryAsync(
        [AsParameters] GetCategoryRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CategoryId == request.CategoryId, cancellationToken: cancellationToken);
        if (category == null) return TypedResults.NotFound();
        return TypedResults.Ok(category);
    }

    private static async Task<Ok<Dictionary<long, long>>> GetCategoryThreadsCountAsync(
        [AsParameters] GetCategoryThreadsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from c in dbContext.Categories
            from t in c.Threads
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, request.CategoryIds.ToArray())
            group t by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<List<Thread>>>> GetCategoryThreadsAsync(
        [AsParameters] GetCategoryThreadsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        IQueryable<Thread> query;
        if (request.Sort?.Field == GetCategoryThreadsRequest.GetCategoryThreadsRequestSortType.Activity)
        {
            var latestPosts =
                from t in dbContext.Threads
                from p in t.Posts
                where t.CategoryId == request.CategoryId
                group p by t.ThreadId
                into g
                select new { ThreadId = g.Key, PostId = g.Max(p => p.PostId) };

            var q =
                from lp in latestPosts
                join t in dbContext.Threads on lp.ThreadId equals t.ThreadId
                join p in dbContext.Posts
                    on new { lp.ThreadId, lp.PostId }
                    equals new { p.ThreadId, p.PostId }
                    into g
                from p in g.DefaultIfEmpty()
                select new { t, p };
            
            q = request.Sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.p.Created)
                : q.OrderByDescending(e => e.p.Created);
            
            query = q.AsNoTracking().Select(e => e.t);
        }
        else
        {
            query = dbContext.Threads
                .AsNoTracking()
                .OrderBy(e => e.ThreadId)
                .Where(e => e.CategoryId == request.CategoryId);
        }


        if (request.Cursor != null)
        {
            query = query.Skip((int)request.Cursor.Value);
        }

        var threads = await query
            .Take(request.Limit ?? 100)
            .ToListAsyncLinqToDB(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(threads);
    }

    private static async Task<Ok<long>> CreateCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateCategoryRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var category = new Category
        {
            ForumId = request.ForumId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(category.CategoryId);
    }
}