using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Paging;
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

        var query = dbContext.Posts
            .AsNoTracking();

        if (request.TopicLatest)
        {
            query = query.OrderBy(e => e.TopicId).ThenByDescending(e => e.PostId).AsQueryable();
        }
        else
        {
            query = query.OrderBy(e => e.PostId).AsQueryable();
        }

        if (request.TopicIds != null)
        {
            query = query.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.TopicId, request.TopicIds.ToArray()));
        }

        if (request.Cursor != null)
        {
            query = query.Where(e => e.TopicId > request.Cursor);
        }

        if (request.TopicLatest)
        {
            query = query.Select(e => new Post
            {
                PostId = e.PostId.SqlDistinctOn(e.TopicId),
                TopicId = e.TopicId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Content = e.Content
            });
        }

        var posts = await query.Take(request.Limit ?? 100).ToListAsyncLinqToDB(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Post> { Items = posts });
    }
}