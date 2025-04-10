using System.Data;
using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using CoreService.Infrastructure.Persistence;
using Mapster;
using Wolverine;
using Thread = CoreService.Domain.Entities.Thread;
using OneOf;

namespace CoreService.Presentation.Apis;

public static class ThreadApi
{
    public static IEndpointRouteBuilder MapThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .AddFluentValidationAutoValidation();

        api.MapGet("{threadId}", GetThreadAsync);
        api.MapGet("{threadIds}/posts/count", GetThreadPostsCountAsync);
        api.MapGet("{threadIds}/posts/latest", GetThreadPostsLatestAsync);
        api.MapPost(string.Empty, CreateThreadAsync).RequireAuthorization();
        api.MapPost("{threadId}/posts", CreatePostAsync).RequireAuthorization();
        return app;
    }

    private static async Task<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>>> GetThreadAsync(
        [FromRoute] ThreadId threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadQuery
        {
            ThreadId = threadId
        };

        var result = await messageBus.InvokeAsync<OneOf<ThreadDto, ThreadNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<List<PostDto>>> GetThreadPostsLatestAsync(
        [AsParameters] GetThreadPostsLatestRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from p in dbContext.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.ThreadId, ids.ToSqlGuid<Guid, ThreadId>())
            orderby p.ThreadId, p.PostId descending
            select new PostDto
            {
                PostId = p.PostId.SqlDistinctOn(p.ThreadId),
                ThreadId = p.ThreadId,
                Content = p.Content,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
            };
        return TypedResults.Ok(await query.ToListAsyncLinqToDB(cancellationToken));
    }

    private static async Task<Ok<Dictionary<ThreadId, long>>> GetThreadPostsCountAsync(
        [AsParameters] GetThreadPostsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var ids = request.ThreadIds.Select(x => x.Value).ToArray();
        var query =
            from t in dbContext.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, ids.ToSqlGuid<Guid, ThreadId>())
            group p by t.ThreadId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Ok<ThreadId>> CreateThreadAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateThreadRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var thread = new Thread
        {
            ThreadId = ThreadId.From(Guid.CreateVersion7()),
            PostIdSeq = 0,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Threads.AddAsync(thread, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(thread.ThreadId);
    }

    private static async Task<Results<NotFound, Ok<PostId>>> CreatePostAsync(
        ClaimsPrincipal claimsPrincipal,
        [AsParameters] CreatePostRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var post = new Post
        {
            ThreadId = request.ThreadId,
            Content = request.Body.Content,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await using (var transaction =
                     await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
        {
            var thread = await dbContext.Threads
                .Where(e => e.ThreadId == request.ThreadId)
                .Select(e => new { e.PostIdSeq })
                .QueryHint(PostgreSQLHints.ForUpdate)
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);
            if (thread == null) return TypedResults.NotFound();
            post.PostId = PostId.From(thread.PostIdSeq + 1);
            await dbContext.Posts.AddAsync(post, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Threads
                .Where(e => e.ThreadId == request.ThreadId)
                .Set(e => e.PostIdSeq, post.PostId.Value)
                .UpdateAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        return TypedResults.Ok(post.PostId);
    }
}