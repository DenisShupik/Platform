using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Extensions;
using SharedKernel.Sorting;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;
using OneOf;
using SharedKernel.Application.Abstractions;

namespace CoreService.Presentation.Apis;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetCategoriesAsync);
        api.MapGet("{categoryId}", GetCategoryAsync);
        api.MapGet("{categoryIds}/posts/count", GetCategoriesPostsCountAsync);
        api.MapGet("{categoryIds}/posts/latest", GetCategoriesPostsLatestAsync);
        api.MapGet("{categoryIds}/threads/count", GetCategoryThreadsCountAsync);
        api.MapGet("{categoryId}/threads", GetCategoryThreadsAsync);
        api.MapPost(string.Empty, CreateCategoryAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] ForumId? forumId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            ForumId = forumId
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<CategoryDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>> GetCategoryAsync(
        [FromRoute] CategoryId categoryId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryQuery
        {
            CategoryId = categoryId
        };

        var result = await messageBus.InvokeAsync<OneOf<CategoryDto, CategoryNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<Dictionary<CategoryId, long>>> GetCategoriesPostsCountAsync(
        [FromRoute] GuidIdList<CategoryId> categoryIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPostsCountQuery
        {
            CategoryIds = categoryIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<CategoryId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Dictionary<CategoryId, PostDto>>> GetCategoriesPostsLatestAsync(
        [FromRoute] GuidIdList<CategoryId> categoryIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPostsLatestQuery
        {
            CategoryIds = categoryIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<CategoryId, PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Dictionary<CategoryId, long>>> GetCategoryThreadsCountAsync(
        [AsParameters] GetCategoryThreadsCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var ids = request.CategoryIds.Select(x => x.Value).ToArray();
        var query =
            from c in dbContext.Categories
            from t in c.Threads
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(c.CategoryId, ids.ToSqlGuid<Guid, CategoryId>())
            group t by c.CategoryId
            into g
            select new { g.Key, Value = g.LongCount() };

        return TypedResults.Ok(await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken));
    }

    private static async Task<Results<NotFound, Ok<IReadOnlyList<ThreadDto>>>> GetCategoryThreadsAsync(
        [FromRoute] CategoryId categoryId,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] SortCriteria<GetCategoryThreadsQuery.GetCategoryThreadsRequestSortType>? sort,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryThreadsQuery
        {
            CategoryId = categoryId,
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ThreadDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<CategoryId>> CreateCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateCategoryRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var category = new Category
        {
            CategoryId = CategoryId.From(Guid.CreateVersion7()),
            ForumId = request.ForumId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(category.CategoryId);
    }
}