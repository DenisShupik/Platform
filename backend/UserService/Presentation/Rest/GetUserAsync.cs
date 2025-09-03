using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<UserDto>, NotFound<UserNotFoundError>>> GetUserAsync(
        [FromRoute] UserId userId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserQuery
        {
            UserId = userId
        };

        var result = await messageBus.InvokeAsync<OneOf<UserDto, UserNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<UserDto>, NotFound<UserNotFoundError>>>(
            userDto => TypedResults.Ok(userDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}