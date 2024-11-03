using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Extensions;
using SharedKernel.Paging;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using TopicService.Application.DTOs;
using TopicService.Domain.Entities;
using TopicService.Infrastructure.Persistence;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("{categoryIds}/stats", GetCategoryStatsAsync).AllowAnonymous();
        api.MapGet("{categoryId}", GetCategoryAsync).AllowAnonymous();
        api.MapGet("{categoryId}/topics/count", GetCategoryTopicsCountAsync).AllowAnonymous();
        api.MapGet("{categoryId}/topics", GetCategoryTopicsAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateCategoryAsync);

        return app;
    }

    private static async Task<Ok<List<CategoryStats>>> GetCategoryStatsAsync(
        [AsParameters] GetCategoriesStatsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from c in dbContext.Categories
            join t in dbContext.Topics on c.CategoryId equals t.CategoryId into tg
            from t in tg.DefaultIfEmpty()
            join p in dbContext.Posts on t.TopicId equals p.TopicId into pg
            from p in pg.DefaultIfEmpty()
            where request.CategoryIds.Contains(c.CategoryId)
            group new { t, p } by c.CategoryId
            into g
            select new CategoryStats
            {
                CategoryId = g.Key,
                TopicCount = g.Select(e => e.t.TopicId).Distinct().Count(),
                // TODO: подумать как убрать не нужный здесь DISTINCT
                PostCount = g.Select(e => e.p.PostId).Distinct().Count()
            };


        return TypedResults.Ok(await query.ToListAsync(cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<Category>>> GetCategoryAsync(
        [AsParameters] GetCategoryRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var section = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CategoryId == request.CategoryId, cancellationToken: cancellationToken);
        if (section == null) return TypedResults.NotFound();
        return TypedResults.Ok(section);
    }

    private static async Task<Results<NotFound, Ok<long>>> GetCategoryTopicsCountAsync(
        [AsParameters] GetCategoryTopicsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = await dbContext.Categories
            .AsNoTracking()
            .Where(e => e.CategoryId == request.CategoryId)
            .Select(e => new { TopicCount = e.Topics.LongCount() })
            .FirstOrDefaultAsync(cancellationToken);

        if (query == null) return TypedResults.NotFound();

        return TypedResults.Ok(query.TopicCount);
    }

    private static async Task<Results<NotFound, Ok<KeysetPageResponse<Topic>>>> GetCategoryTopicsAsync(
        [AsParameters] GetCategoryTopicsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Topics
            .AsNoTracking()
            .OrderBy(e => e.TopicId)
            .Where(e => e.CategoryId == request.CategoryId);

        if (request.Cursor != null)
        {
            query = query.Where(e => e.TopicId > request.Cursor);
        }

        var topics = await query.Take(request.Limit ?? 100).ToListAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Topic> { Items = topics });
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
            SectionId = request.SectionId,
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