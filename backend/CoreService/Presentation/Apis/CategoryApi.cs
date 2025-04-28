using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Presentation.Extensions;

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
        api.MapGet("{categoryIds}/threads/count", GetCategoriesThreadsCountAsync);
        api.MapGet("{categoryId}/threads", GetCategoryThreadsAsync);
        api.MapPost(string.Empty, CreateCategoryAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] ForumId? forumId,
        [FromQuery] CategoryTitle? title,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            ForumId = forumId,
            Title = title
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
            categoryDto => TypedResults.Ok(categoryDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<Dictionary<CategoryId, long>>> GetCategoriesPostsCountAsync(
        [FromRoute] IdList<CategoryId> categoryIds,
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
        [FromRoute] IdList<CategoryId> categoryIds,
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

    private static async Task<Ok<Dictionary<CategoryId, long>>> GetCategoriesThreadsCountAsync(
        [FromRoute] IdList<CategoryId> categoryIds,
        [FromQuery] bool? includeDraft,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesThreadsCountQuery
        {
            CategoryIds = categoryIds,
            IncludeDraft = includeDraft ?? false
        };

        var result = await messageBus.InvokeAsync<Dictionary<CategoryId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NotFound, Ok<IReadOnlyList<ThreadDto>>>> GetCategoryThreadsAsync(
        [FromRoute] CategoryId categoryId,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] SortCriteria<GetCategoryThreadsQuery.GetCategoryThreadsRequestSortType>? sort,
        [FromQuery] bool? includeDraft,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryThreadsQuery
        {
            CategoryId = categoryId,
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            Sort = sort,
            IncludeDraft = includeDraft ?? false
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ThreadDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NotFound<ForumNotFoundError>, Ok<CategoryId>>> CreateCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateCategoryRequestBody body,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateCategoryCommand
        {
            ForumId = body.ForumId,
            Title = body.Title,
            UserId = userId
        };
        var result = await messageBus.InvokeAsync<CreateCategoryCommandResult>(command, cancellationToken);

        return result.Match<Results<NotFound<ForumNotFoundError>, Ok<CategoryId>>>(
            notFound => TypedResults.NotFound(notFound),
            categoryId => TypedResults.Ok(categoryId)
        );
    }
}