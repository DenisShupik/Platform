using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Extensions;
using SharedKernel.Paging;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Application.DTOs;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Thread = CoreService.Domain.Entities.Thread;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("{categoryIds}/posts", GetCategoryPostLatestAsync).AllowAnonymous();
        api.MapGet("{categoryIds}/stats", GetCategoryStatsAsync).AllowAnonymous();
        api.MapGet("{categoryId}", GetCategoryAsync).AllowAnonymous();
        api.MapGet("{categoryId}/threads/count", GetCategoryThreadsCountAsync).AllowAnonymous();
        api.MapGet("{categoryId}/threads", GetCategoryThreadsAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateCategoryAsync);

        return app;
    }

    private static async Task<Ok<List<GetCategoryPostLatestResponse>>> GetCategoryPostLatestAsync(
        [AsParameters] GetCategoryPostLatestRequest request,
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
            .Select(e => new GetCategoryPostLatestResponse
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

    private static async Task<Ok<List<GetCategoriesStatsResponse>>> GetCategoryStatsAsync(
        [AsParameters] GetCategoriesStatsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from c in dbContext.Categories
            join t in dbContext.Threads on c.CategoryId equals t.CategoryId into tg
            from t in tg.DefaultIfEmpty()
            join p in dbContext.Posts on t.ThreadId equals p.ThreadId into pg
            from p in pg.DefaultIfEmpty()
            where request.CategoryIds.Contains(c.CategoryId)
            group new { t, p } by c.CategoryId
            into g
            select new GetCategoriesStatsResponse
            {
                CategoryId = g.Key,
                ThreadCount = g.Select(e => e.t.ThreadId).Distinct().Count(),
                // TODO: подумать как убрать не нужный здесь DISTINCT
                PostCount = g.Select(e => e.p.PostId).Distinct().Count()
            };


        return TypedResults.Ok(await EntityFrameworkQueryableExtensions.ToListAsync(query, cancellationToken));
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

    private static async Task<Results<NotFound, Ok<long>>> GetCategoryThreadsCountAsync(
        [AsParameters] GetCategoryThreadsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(dbContext.Categories
            .AsNoTracking()
            .Where(e => e.CategoryId == request.CategoryId)
            .Select(e => new { ThreadCount = e.Threads.LongCount() }), cancellationToken);

        if (query == null) return TypedResults.NotFound();

        return TypedResults.Ok(query.ThreadCount);
    }

    private static async Task<Results<NotFound, Ok<KeysetPageResponse<Thread>>>> GetCategoryThreadsAsync(
        [AsParameters] GetCategoryThreadsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Threads
            .AsNoTracking()
            .OrderBy(e => e.ThreadId)
            .Where(e => e.CategoryId == request.CategoryId);

        if (request.Cursor != null)
        {
            query = query.Where(e => e.ThreadId > request.Cursor);
        }

        var threads =
            await EntityFrameworkQueryableExtensions.ToListAsync(query.Take(request.Limit ?? 100), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Thread> { Items = threads });
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