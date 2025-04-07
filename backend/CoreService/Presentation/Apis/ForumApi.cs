using System.Security.Claims;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Paging;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using SharedKernel.Sorting;

public static class ForumApi
{
    public static IEndpointRouteBuilder MapForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetForumsCountAsync).AllowAnonymous();
        api.MapGet(string.Empty, GetForumsAsync).AllowAnonymous();
        api.MapGet("{forumId}", GetForumAsync).AllowAnonymous();
        api.MapGet("{forumIds}/categories/count", GetForumCategoriesCountAsync).AllowAnonymous();
        api.MapGet("{forumId}/categories", GetForumCategoriesAsync).AllowAnonymous();
        api.MapGet("{forumIds}/categories/latest-by-post", GetForumsCategoriesLatestByPostAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateForumAsync);

        return app;
    }

    private static async Task<Ok<Dictionary<long, Category[]>>> GetForumsCategoriesLatestByPostAsync(
        [AsParameters] GetForumsCategoriesLatestByPostRequest latestByPostRequest,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var latestPostCreatedCte =
            (
                from c in dbContext.Categories
                from t in c.Threads
                from p in t.Posts
                where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.ForumId, latestByPostRequest.ForumIds.ToArray())
                group p by new { c.ForumId, c.CategoryId }
                into g
                select new
                {
                    g.Key.ForumId,
                    g.Key.CategoryId,
                    Created = g.Max(p => p.Created)
                }
            )
            .AsCte();

        var rankedCategoriesCte =
            (
                from lpc in latestPostCreatedCte
                select new
                {
                    lpc.CategoryId,
                    lpc.ForumId,
                    Rank = Sql.Ext.RowNumber()
                        .Over()
                        .PartitionBy(lpc.ForumId)
                        .OrderByDesc(lpc.Created)
                        .ToValue(),
                }
            )
            .AsCte();

        var result = (
                await (
                        from rc in rankedCategoriesCte
                        join c in dbContext.Categories
                            on rc.CategoryId equals c.CategoryId
                        where rc.Rank <= 5
                        orderby rc.ForumId, rc.Rank
                        select c
                    )
                    .ToArrayAsyncLinqToDB(cancellationToken)
            )
            .GroupBy(e => e.ForumId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<KeysetPageResponse<Category>>> GetForumCategoriesAsync(
        [AsParameters] GetForumCategoriesRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Categories
            .AsNoTracking()
            .OrderBy(e => e.CategoryId)
            .Where(e => e.ForumId == request.ForumId);

        if (request.Cursor != null)
        {
            query = query.Where(e => e.CategoryId > request.Cursor);
        }

        var categories = await query.Take(request.Limit ?? 100).ToListAsyncEF(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Category> { Items = categories });
    }

    private static async Task<Ok<Dictionary<long, long>>> GetForumCategoriesCountAsync(
        [AsParameters] GetForumCategoriesCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query =
            from f in dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, request.ForumIds.ToArray())
            group c by f.ForumId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<long>>> GetForumsCountAsync(
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var count = await dbContext.Forums.LongCountAsyncLinqToDB(cancellationToken);

        return TypedResults.Ok(count);
    }

    private static async Task<Ok<KeysetPageResponse<Forum>>> GetForumsAsync(
        [AsParameters] GetForumsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        IQueryable<Forum> query;
        if (request.Sort != null && request.Sort.Field == GetForumsRequest.SortType.latestPost)
        {
            var q = (
                    from f in dbContext.Forums
                    join c in dbContext.Categories on f.ForumId equals c.ForumId into gc
                    from c in gc.DefaultIfEmpty()
                    join t in dbContext.Threads on c.CategoryId equals t.CategoryId into gt
                    from t in gt.DefaultIfEmpty()
                    join p in dbContext.Posts on t.ThreadId equals p.ThreadId into gp
                    from p in gp.DefaultIfEmpty()
                    group p by new { f.ForumId, f.Title, f.Created, f.CreatedBy }
                    into g
                    select new
                    {
                        g.Key.ForumId,
                        g.Key.Title,
                        g.Key.Created,
                        g.Key.CreatedBy,
                        LastPostDate = g.Max(p => (DateTime?)p.Created)
                    }
                )
                .AsCte();

            q = request.Sort.Order == SortOrderType.Ascending
                ? q.OrderBy(e => e.LastPostDate.SqlIsNotNull()).ThenBy(e => e.LastPostDate ?? e.Created)
                : q.OrderByDescending(e => e.LastPostDate.SqlIsNotNull())
                    .ThenByDescending(e => e.LastPostDate ?? e.Created);

            query = q.Select(e => new Forum
            {
                ForumId = e.ForumId,
                Title = e.Title,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            });
        }
        else
        {
            query = dbContext.Forums
                .AsNoTracking()
                .OrderBy(e => e.ForumId)
                .AsQueryable();
        }

        if (request.Cursor != null)
        {
            query = query.Skip((int)request.Cursor);
        }

        var forums =
            await query.Take(request.Limit ?? 100).ToListAsyncLinqToDB(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Forum> { Items = forums });
    }

    private static async Task<Results<NotFound, Ok<Forum>>> GetForumAsync(
        [AsParameters] GetForumRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var forum = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(dbContext.Forums
            .AsNoTracking(), e => e.ForumId == request.ForumId, cancellationToken);
        if (forum == null) return TypedResults.NotFound();
        return TypedResults.Ok(forum);
    }

    private static async Task<Ok<long>> CreateForumAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateForumRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var forum = new Forum
        {
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Forums.AddAsync(forum, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(forum.ForumId);
    }
}