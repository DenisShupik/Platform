using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace UserService.Presentation.Rest;

public static class UserApi
{
    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/users");

        api.MapGet(string.Empty, GetUsersPagedAsync);
        api.MapGet("{userId}", GetUserByIdAsync);
        api.MapGet("batch/{userIds}", GetUsersBulkAsync);
        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<UserDto>>, BadRequest<string>>> GetUsersPagedAsync(
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] SortCriteriaList<GetUsersPagedQuery.GetUsersPagedQuerySortType>? sort,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersPagedQuery
        {
            Offset = offset,
            Limit = limit,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<UserDto>, NotFound<UserNotFoundError>>> GetUserByIdAsync(
        [FromRoute] UserId userId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserByIdQuery
        {
            UserId = userId
        };

        var result = await messageBus.InvokeAsync<OneOf<UserDto, UserNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<UserDto>, NotFound<UserNotFoundError>>>(
            userDto => TypedResults.Ok(userDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<IReadOnlyList<UserDto>>> GetUsersBulkAsync(
        [FromRoute] IdSet<UserId> userIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersBulkQuery
        {
            UserIds = userIds
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}