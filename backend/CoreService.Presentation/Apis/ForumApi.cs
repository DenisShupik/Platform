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
        api.MapPost(string.Empty, CreateForumAsync);

        return app;
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

    private static async Task<Ok<Dictionary<long,long>>> GetForumCategoriesCountAsync(
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

        var query = dbContext.Forums
            .AsNoTracking()
            .OrderBy(e => e.ForumId)
            .Include(e => e.Categories)
            .AsQueryable();

        if (request.Cursor != null)
        {
            query = query.Where(e => e.ForumId > request.Cursor);
        }

        var forums = await EntityFrameworkQueryableExtensions.ToListAsync(query.Take(request.Limit ?? 100), cancellationToken);
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