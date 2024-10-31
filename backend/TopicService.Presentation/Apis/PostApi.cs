using System.Data;
using System.Security.Claims;
using Common.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using TopicService.Application.DTOs;
using TopicService.Domain.Entities;
using TopicService.Infrastructure.Persistence;

namespace TopicService.Presentation.Apis;

public static class PostApi
{
    public static IEndpointRouteBuilder MapPostApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/posts")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        // api.MapGet("{topicId}", GetTopicAsync);
        // api.MapGet("{topicId}/posts/count", GetPostsCountAsync);
        // api.MapGet("{topicId}/posts", GetPostsAsync);
        api.MapPost(string.Empty, CreatePostAsync);
        return app;
    }

    // private static async Task<Results<NotFound, Ok<Topic>>> GetTopicAsync(
    //     [AsParameters] GetTopicRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //     var topic = await dbContext.Topics
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(e => e.TopicId == request.TopicId, cancellationToken: cancellationToken);
    //     if (topic == null) return TypedResults.NotFound();
    //     return TypedResults.Ok(topic);
    // }
    //
    // private static async Task<Results<NotFound, Ok<int>>> GetPostsCountAsync(
    //     [AsParameters] GetPostsCountRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //     var query = await dbContext.Posts
    //         .Where(e => e.TopicId == request.TopicId)
    //         .CountAsync(cancellationToken);
    //
    //     if (query == 0) return TypedResults.NotFound();
    //
    //     return TypedResults.Ok(query);
    // }
    //
    // private static async Task<Ok<KeysetPageResponse<Post>>> GetPostsAsync(
    //     [AsParameters] GetPostsRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //
    //     var query = dbContext.Posts
    //         .AsNoTracking()
    //         .OrderBy(e => e.PostId)
    //         .Where(e => e.TopicId == request.TopicId);
    //
    //     if (request.Cursor != null)
    //     {
    //         query = query.Where(e => e.TopicId > request.Cursor);
    //     }
    //
    //     var posts = await query.Take(request.PageSize ?? 100).ToListAsync(cancellationToken);
    //     await dbContext.SaveChangesAsync(cancellationToken);
    //     return TypedResults.Ok(new KeysetPageResponse<Post> { Items = posts });
    // }

    private static async Task<Results<NotFound, Ok<long>>> CreatePostAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreatePostRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var post = new Post
        {
            TopicId = request.TopicId,
            Content = request.Content,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead,
                         cancellationToken: cancellationToken))
        {
            var topic = await dbContext.Topics
                .Where(e => e.TopicId == request.TopicId)
                .Select(e => new { e.PostIdSeq })
                .QueryHint(PostgreSQLHints.ForUpdate)
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);
            if (topic == null) return TypedResults.NotFound();
            post.PostId = topic.PostIdSeq + 1;
            await dbContext.Posts.AddAsync(post, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Topics
                .Where(e => e.TopicId == request.TopicId)
                .Set(e => e.PostIdSeq, post.PostId)
                .UpdateAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        return TypedResults.Ok(post.PostId);
    }
}