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
using CoreService.Application.DTOs;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Presentation.Apis;

public static class ThreadApi
{
    public static IEndpointRouteBuilder MapThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("{threadId}", GetThreadAsync);
        api.MapGet("{threadIds}/posts/count", GetThreadPostsCountAsync);
        api.MapGet("{threadIds}/posts", GetThreadPostsAsync).AllowAnonymous();
        api.MapPost(string.Empty, CreateThreadAsync);
        api.MapPost("{threadId}/posts", CreatePostAsync);
        return app;
    }

    private static async Task<Results<NotFound, Ok<Thread>>> GetThreadAsync(
        [AsParameters] GetThreadRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var thread = await dbContext.Threads
            .AsNoTracking()
            .FirstOrDefaultAsyncEF(e => e.ThreadId == request.ThreadId, cancellationToken);
        if (thread == null) return TypedResults.NotFound();
        return TypedResults.Ok(thread);
    }

    private static async Task<Ok<Dictionary<long, long>>> GetThreadPostsCountAsync(
        [AsParameters] GetThreadPostsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query =
            from t in dbContext.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, request.ThreadIds.ToArray())
            group p by t.ThreadId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Ok<KeysetPageResponse<Post>>> GetThreadPostsAsync(
        [AsParameters] GetThreadPostsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query =
            from t in dbContext.Threads
            from p in t.Posts
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, request.ThreadIds.ToArray())
            select new { t, p };

        if (request.Cursor != null)
        {
            query = query.Where(e => e.p.PostId > request.Cursor);
        }

        if (request.Latest != null && request.Latest.Value)
        {
            query = query
                .OrderBy(e => e.t.ThreadId)
                .ThenByDescending(e => e.p.PostId);
        }
        else
        {
            query = query.OrderBy(e => e.t.ThreadId);
        }

        var posts = query
            .Select(e => new Post
                {
                    PostId = request.Latest != null && request.Latest.Value
                        ? e.p.PostId.SqlDistinctOn(e.t.ThreadId)
                        : e.p.PostId,
                    ThreadId = e.p.ThreadId,
                    Created = e.p.Created,
                    CreatedBy = e.p.CreatedBy,
                    Content = e.p.Content
                }
            );

        // if (request.Sort != null)
        // {
        //     if (request.Sort.Field == GetThreadPostsRequest.PostSortType.Id)
        //     {
        //         query = request.Sort.Order == SortOrderType.Ascending
        //             ? query.OrderBy(e => e.ThreadId)
        //             : query.OrderByDescending(e => e.ThreadId);
        //     }
        // }

        return TypedResults.Ok(new KeysetPageResponse<Post>
            { Items = await posts.ToListAsyncLinqToDB(cancellationToken) });
    }

    private static async Task<Ok<long>> CreateThreadAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateThreadRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var thread = new Thread
        {
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
            ThreadId = request.ThreadId,
            Content = request.Body.Content,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead,
                         cancellationToken: cancellationToken))
        {
            var thread = await dbContext.Threads
                .Where(e => e.ThreadId == request.ThreadId)
                .Select(e => new { e.PostIdSeq })
                .QueryHint(PostgreSQLHints.ForUpdate)
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);
            if (thread == null) return TypedResults.NotFound();
            post.PostId = thread.PostIdSeq + 1;
            await dbContext.Posts.AddAsync(post, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Threads
                .Where(e => e.ThreadId == request.ThreadId)
                .Set(e => e.PostIdSeq, post.PostId)
                .UpdateAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        return TypedResults.Ok(post.PostId);
    }
}