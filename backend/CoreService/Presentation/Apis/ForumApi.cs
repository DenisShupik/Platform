using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using CoreService.Infrastructure.Persistence;
using JasperFx.Core;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Sorting;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;
using OneOf;

namespace CoreService.Presentation.Apis;

public static class ForumApi
{
    public static IEndpointRouteBuilder MapForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetForumsCountAsync);
        api.MapGet(string.Empty, GetForumsAsync);
        api.MapGet("{forumId}", GetForumAsync);
        api.MapGet("{forumIds}/categories/count", GetForumCategoriesCountAsync);
        api.MapGet("{forumIds}/categories/latest-by-post", GetForumsCategoriesLatestByPostAsync);
        api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<Dictionary<ForumId, CategoryDto[]>>> GetForumsCategoriesLatestByPostAsync(
        [AsParameters] GetForumsCategoriesLatestByPostRequest latestByPostRequest,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var ids = latestByPostRequest.ForumIds.Select(x=>x.Value).ToArray();
        var latestPostCreatedCte =
            (
                from c in dbContext.Categories
                from t in c.Threads
                from p in t.Posts
                where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.ForumId, ids.ToSqlGuid<Guid,ForumId>())
                group p by new { c.ForumId, c.CategoryId }
                into g
                select new
                {
                    g.Key.ForumId,
                    g.Key.CategoryId,
                    Created = g.Max(p => p.Created)
                }
            )
            .AsCte();

        var rankedCategoriesCte =
            (
                from lpc in latestPostCreatedCte
                select new
                {
                    lpc.CategoryId,
                    lpc.ForumId,
                    Rank = Sql.Ext.RowNumber()
                        .Over()
                        .PartitionBy(lpc.ForumId)
                        .OrderByDesc(lpc.Created)
                        .ToValue(),
                }
            )
            .AsCte();

        var result = (
                await (
                        from rc in rankedCategoriesCte
                        join c in dbContext.Categories
                            on rc.CategoryId equals c.CategoryId
                        where rc.Rank <= 5
                        orderby rc.ForumId, rc.Rank
                        select c
                    )
                    .ProjectToType<CategoryDto>()
                    .ToArrayAsyncLinqToDB(cancellationToken)
            )
            .GroupBy(e => e.ForumId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<Dictionary<ForumId, long>>> GetForumCategoriesCountAsync(
        [AsParameters] GetForumCategoriesCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var forums = request.ForumIds.Map(x=>x.Value).ToArray();
        var query =
            from f in dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, forums.ToSqlGuid<Guid,ForumId>())
            group c by f.ForumId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<long>>> GetForumsCountAsync(
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var count = await dbContext.Forums.LongCountAsyncLinqToDB(cancellationToken);

        return TypedResults.Ok(count);
    }

    private static async Task<Ok<IReadOnlyList<ForumDto>>> GetForumsAsync(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] SortCriteria<GetForumsQuery.SortType>? sort,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ForumDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>> GetForumAsync(
        [FromRoute] ForumId forumId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumQuery
        {
            ForumId = forumId
        };

        var result = await messageBus.InvokeAsync<OneOf<ForumDto,ForumNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<ForumId>> CreateForumAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateForumRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var forum = new Forum
        {
            ForumId = ForumId.From(Guid.CreateVersion7()),
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Forums.AddAsync(forum, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(forum.ForumId);
    }
}