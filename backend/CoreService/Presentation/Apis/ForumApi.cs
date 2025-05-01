using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.Enums;
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
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Presentation.Extensions;

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
        api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync);
        api.MapGet("{forumIds}/categories/latest", GetForumsCategoriesLatestAsync);
        api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<Dictionary<ForumId, CategoryDto[]>>> GetForumsCategoriesLatestAsync(
        [FromRoute] IdList<ForumId> forumIds,
        [FromQuery] int? count,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCategoriesLatestQuery
        {
            ForumIds = forumIds,
            Count = count ?? 5
        };

        var result = await messageBus.InvokeAsync<Dictionary<ForumId, CategoryDto[]>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Dictionary<ForumId, long>>> GetForumsCategoriesCountAsync(
        [FromRoute] IdList<ForumId> forumIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCategoriesCountQuery
        {
            ForumIds = forumIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<ForumId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<long>> GetForumsCountAsync(
        [FromQuery] UserId? createdBy,
        [FromQuery] ForumContainsFilter? contains,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCountQuery
        {
            CreatedBy = createdBy,
            Contains = contains
        };

        var result = await messageBus.InvokeAsync<long>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<ForumDto>>> GetForumsAsync(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] SortCriteria<GetForumsQuery.SortType>? sort,
        [FromQuery] ForumTitle? title,
        [FromQuery] UserId? createdBy,
        [FromQuery] ForumContainsFilter? contains,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            Sort = sort,
            Title = title,
            CreatedBy = createdBy,
            Contains = contains
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

        var result = await messageBus.InvokeAsync<OneOf<ForumDto, ForumNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>>(
            forumDto => TypedResults.Ok(forumDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<ForumId>> CreateForumAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateForumRequestBody body,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateForumCommand
        {
            Title = body.Title,
            CreatedBy = userId
        };
        var result = await messageBus.InvokeAsync<ForumId>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}