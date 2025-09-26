using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.Errors;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<UserDto>, NotFound<UserNotFoundError>>> GetUserAsync(
        GetUserRequest request,
        [FromServices] GetUserQueryHandler<UserDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserQuery<UserDto>
        {
            UserId = request.UserId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<UserDto>, NotFound<UserNotFoundError>>>(
            userDto => TypedResults.Ok(userDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}