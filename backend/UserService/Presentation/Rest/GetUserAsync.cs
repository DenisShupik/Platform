using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Errors;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

using Response = Results<Ok<UserDto>, NotFound<UserNotFoundError>>;

public static partial class Api
{
    /// <summary>
    /// Получить пользователя
    /// </summary>
    public static async Task<Response> GetUserAsync(
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

        return result.Match<Response>(
            userDto => TypedResults.Ok(userDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}