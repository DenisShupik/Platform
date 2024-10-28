using System.Security.Claims;
using Common.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using TopicService.Application.DTOs;
using TopicService.Domain.Entities;
using TopicService.Infrastructure.Persistence;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/sections/{sectionId}/categories")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

     
        api.MapPost(string.Empty, CreateCategoryAsync);
        
        return app;
    }
    
    private static async Task<Ok<long>> CreateCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        [AsParameters] CreateCategoryRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var category = new Category
        {
            SectionId = request.SectionId,
            Title = request.Body.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(category.CategoryId);
    }
}