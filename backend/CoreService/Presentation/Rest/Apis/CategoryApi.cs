using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;
using CreateCategoryRequestBody = CoreService.Presentation.Rest.Dtos.CreateCategoryRequestBody;

namespace CoreService.Presentation.Rest.Apis;

public static class CategoryApi
{
    public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetCategoriesPagedAsync);
        api.MapGet("{categoryId}", GetCategoryAsync);
        api.MapGet("{categoryIds}/posts/count", GetCategoriesPostsCountAsync);
        api.MapGet("{categoryIds}/posts/latest", GetCategoriesPostsLatestAsync);
        api.MapGet("{categoryIds}/threads/count", GetCategoriesThreadsCountAsync);
        api.MapGet("{categoryId}/threads", GetCategoryThreadsAsync);
        api.MapPost(string.Empty, CreateCategoryAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<GetCategoriesPagedQueryResult>> GetCategoriesPagedAsync(
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] IdSet<ForumId>? forumIds,
        [FromQuery] CategoryTitle? title,
        [FromQuery] SortCriteriaList<GetCategoriesPagedQuery.GetCategoriesPagedQuerySortType>? sort,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPagedQuery
        {
            Offset = offset,
            Limit = limit,
            ForumIds = forumIds,
            Title = title,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<GetCategoriesPagedQueryResult>(query, cancellationToken);

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
        [FromRoute] IdSet<CategoryId> categoryIds,
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
        [FromRoute] IdSet<CategoryId> categoryIds,
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
        [FromRoute] IdSet<CategoryId> categoryIds,
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
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] SortCriteria<GetCategoryThreadsQuery.GetCategoryThreadsQuerySortType>? sort,
        [FromQuery] bool? includeDraft,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryThreadsQuery
        {
            CategoryId = categoryId,
            Offset = offset,
            Limit = limit,
            Sort = sort,
            IncludeDraft = includeDraft ?? false
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ThreadDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryId>, NotFound<ForumNotFoundError>>> CreateCategoryAsync(
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
            CreatedBy = userId
        };
        var result = await messageBus.InvokeAsync<CreateCategoryCommandResult>(command, cancellationToken);

        return result.Match<Results<Ok<CategoryId>, NotFound<ForumNotFoundError>>>(
            categoryId => TypedResults.Ok(categoryId),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}