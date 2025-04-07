using CoreService.Application.UseCases;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Paging;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using SharedKernel.Extensions;

namespace CoreService.Presentation.Apis;

public static class PostApi
{
    public static IEndpointRouteBuilder MapPostApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/posts")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetPostsAsync).AllowAnonymous();
        return app;
    }

    private static async Task<Ok<KeysetPageResponse<Post>>> GetPostsAsync(
        [AsParameters] GetPostsRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        IQueryable<Post> query = request.Filter switch
        {
            GetPostsRequest.FilterType.CategoryLatest => from c in dbContext.Categories
                from t in c.Threads
                from p in t.Posts
                where request.Ids == null || Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, request.Ids.ToArray())
                orderby c.CategoryId, p.PostId descending
                select new Post
                {
                    PostId = p.PostId.SqlDistinctOn(c.CategoryId),
                    ThreadId = p.ThreadId,
                    Created = p.Created,
                    CreatedBy = p.CreatedBy,
                    Content = p.Content
                },
            GetPostsRequest.FilterType.ThreadLatest => from t in dbContext.Threads
                from p in t.Posts
                where request.Ids == null || Sql.Ext.PostgreSQL().ValueIsEqualToAny(t.ThreadId, request.Ids.ToArray())
                orderby t.ThreadId, p.PostId descending
                select new Post
                {
                    PostId = p.PostId.SqlDistinctOn(t.ThreadId),
                    ThreadId = p.ThreadId,
                    Created = p.Created,
                    CreatedBy = p.CreatedBy,
                    Content = p.Content
                },
            _ => from p in dbContext.Posts
                where request.Ids == null || Sql.Ext.PostgreSQL().ValueIsEqualToAny(p.PostId, request.Ids.ToArray())
                orderby p.PostId
                select new Post
                {
                    PostId = p.PostId,
                    ThreadId = p.ThreadId,
                    Created = p.Created,
                    CreatedBy = p.CreatedBy,
                    Content = p.Content
                }
        };

        if (request.Cursor != null)
        {
            query = query.Where(e => e.PostId > request.Cursor);
        }
        
        var posts = await query.Take(request.Limit ?? 100).ToListAsyncLinqToDB(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Post> { Items = posts });
    }
}