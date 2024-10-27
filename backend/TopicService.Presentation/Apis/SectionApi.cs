using System.Security.Claims;
using Common.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        api.MapPost(string.Empty, CreateSectionAsync);


        return app;
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