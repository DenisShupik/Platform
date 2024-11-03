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

public static class SectionApi
{
    public static IEndpointRouteBuilder MapSectionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/sections")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetSectionsAsync).AllowAnonymous();
        api.MapGet("{sectionId}", GetSectionAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateSectionAsync);
        
        return app;
    }

    private static async Task<Ok<KeysetPageResponse<Section>>> GetSectionsAsync(
        [AsParameters] GetSectionsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Sections
            .AsNoTracking()
            .OrderBy(e => e.SectionId)
            .Include(e => e.Categories)
            .AsQueryable();

        if (request.Cursor != null)
        {
            query = query.Where(e => e.SectionId > request.Cursor);
        }

        var sections = await query.Take(request.Limit ?? 100).ToListAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Section> { Items = sections });
    }

    private static async Task<Results<NotFound, Ok<Section>>> GetSectionAsync(
        [AsParameters] GetSectionRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var section = await dbContext.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.SectionId == request.SectionId, cancellationToken: cancellationToken);
        if (section == null) return TypedResults.NotFound();
        return TypedResults.Ok(section);
    }

    private static async Task<Ok<long>> CreateSectionAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateSectionRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var topic = new Section
        {
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Sections.AddAsync(topic, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(topic.SectionId);
    }
}