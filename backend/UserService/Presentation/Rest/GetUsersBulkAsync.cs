using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

using Response = Ok<Dictionary<UserId, Result<UserDto, UserNotFoundError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить пользователей по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetUsersBulkAsync(
        GetUsersBulkRequest request,
        [FromServices] GetUsersBulkQueryHandler<UserDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersBulkQuery<UserDto>
        {
            UserIds = request.UserIds
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}