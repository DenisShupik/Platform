using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static class ForumApi
{
    public static IEndpointRouteBuilder MapForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetForumsCountAsync);
        api.MapGet(string.Empty, GetForumsPagedAsync);
        api.MapGet("{forumId}", GetForumAsync);
        api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync);
        api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<Int64>> GetForumsCountAsync(
        [FromQuery] UserId? createdBy,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCountQuery
        {
            CreatedBy = createdBy
        };

        var result = await messageBus.InvokeAsync<Int64>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<GetForumsPagedQueryResult>> GetForumsPagedAsync(
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] SortCriteria<GetForumsPagedQuery.GetForumsPagedQuerySortType>? sort,
        [FromQuery] ForumTitle? title,
        [FromQuery] UserId? createdBy,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsPagedQuery
        {
            Offset = offset,
            Limit = limit,
            Sort = sort,
            Title = title,
            CreatedBy = createdBy
        };

        var result = await messageBus.InvokeAsync<GetForumsPagedQueryResult>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<Dictionary<ForumId, long>>> GetForumsCategoriesCountAsync(
        [FromRoute] IdSet<ForumId> forumIds,
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