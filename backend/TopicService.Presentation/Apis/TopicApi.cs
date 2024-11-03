using System.Data;
using System.Security.Claims;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Paging;
using SharedKernel.Sorting;
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

        api.MapGet("{topicId}", GetTopicAsync);
        api.MapGet("{topicIds}/posts/count", GetTopicPostsCountAsync);
        api.MapGet("{topicId}/posts", GetTopicPostsAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateTopicAsync);
        api.MapPost("{topicId}/posts", CreatePostAsync);
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

    private static async Task<Ok<KeysetPageResponse<GetTopicPostsCountResponse>>> GetTopicPostsCountAsync(
        [AsParameters] GetTopicPostsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query = await dbContext.Posts
            .Where(e => request.TopicIds.Contains(e.TopicId))
            .GroupBy(e => e.TopicId)
            .Select(e => new GetTopicPostsCountResponse
            {
                TopicId = e.Key,
                Count = e.LongCount()
            })
            .ToListAsyncEF(cancellationToken: cancellationToken);
        
        return TypedResults.Ok(new KeysetPageResponse<GetTopicPostsCountResponse> { Items = query });
    }

    private static async Task<Ok<KeysetPageResponse<Post>>> GetTopicPostsAsync(
        [AsParameters] GetTopicPostsRequest request,
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

        if (request.Sort != null)
        {
            if (request.Sort.Field == GetTopicPostsRequest.PostSortType.Id)
            {
                query = request.Sort.Order == SortOrderType.Ascending
                    ? query.OrderBy(e => e.TopicId)
                    : query.OrderByDescending(e => e.TopicId);
            }
        }

        var posts = await query.Take(request.Limit ?? 100).ToListAsyncEF(cancellationToken);
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
            PostIdSeq = 0,
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

    private static async Task<Results<NotFound, Ok<long>>> CreatePostAsync(
        ClaimsPrincipal claimsPrincipal,
        [AsParameters] CreatePostRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var post = new Post
        {
            TopicId = request.TopicId,
            Content = request.Body.Content,
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