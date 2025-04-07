using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.Errors;
using Wolverine;

namespace UserService.Presentation.Apis.Rest;

public static class UserApi
{
    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/users");

        api.MapGet("{userId}", GetUserByIdAsync);
        api.MapGet("batch/{userIds}", GetUsersByIdsAsync);
        return app;
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
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<IReadOnlyList<UserDto>>> GetUsersByIdsAsync(
        [FromRoute] GuidIdList<UserId> userIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersByIdsQuery
        {
            UserIds = userIds
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}