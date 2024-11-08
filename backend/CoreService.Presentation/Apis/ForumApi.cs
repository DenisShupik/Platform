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

public static class ForumApi
{
    public static IEndpointRouteBuilder MapForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetForumsAsync).AllowAnonymous();
        api.MapGet("{forumId}", GetForumAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateForumAsync);
        
        return app;
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

        var forums = await query.Take(request.Limit ?? 100).ToListAsync(cancellationToken);
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
        var forum = await dbContext.Forums
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ForumId == request.ForumId, cancellationToken);
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