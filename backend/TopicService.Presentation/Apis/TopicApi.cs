using System.Security.Claims;
using Common;
using Common.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using TopicService.Application.DTOs;
using TopicService.Domain.Entities;
using TopicService.Infrastructure.Persistence;

namespace TopicService.Presentation.Apis;

public static class TopicApi
{
    public static IEndpointRouteBuilder MapTopicApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/topics")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapPost(string.Empty, CreateTopicAsync);
        
        api.MapGet("{topicId}", GetTopicAsync);
        api.MapGet("{topicId}/posts/count", GetPostsCountAsync);
        api.MapGet("{topicId}/posts", GetPostsAsync);

        return app;
    }

    private static async Task<Results<NotFound, Ok<Topic>>> GetTopicAsync(
        [AsParameters] GetTopicRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var topic = await dbContext.Topics
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TopicId == request.TopicId, cancellationToken: cancellationToken);
        if (topic == null) return TypedResults.NotFound();
        return TypedResults.Ok(topic);
    }
    
    private static async Task<Results<NotFound, Ok<int>>> GetPostsCountAsync(
        [AsParameters] GetPostsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query = await dbContext.Posts
            .Where(e => e.TopicId == request.TopicId)
            .CountAsync(cancellationToken);
    
        if (query == 0) return TypedResults.NotFound();
    
        return TypedResults.Ok(query);
    }
    
    private static async Task<Ok<KeysetPageResponse<Post>>> GetPostsAsync(
        [AsParameters] GetPostsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    
        var query = dbContext.Posts
            .AsNoTracking()
            .OrderBy(e => e.PostId)
            .Where(e => e.TopicId == request.TopicId);
    
        if (request.Cursor != null)
        {
            query = query.Where(e => e.TopicId > request.Cursor);
        }
    
        var posts = await query.Take(request.PageSize ?? 100).ToListAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Post> { Items = posts });
    }
    
    private static async Task<Ok<long>> CreateTopicAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateTopicRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var topic = new Topic
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Topics.AddAsync(topic, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(topic.TopicId);
    }
}