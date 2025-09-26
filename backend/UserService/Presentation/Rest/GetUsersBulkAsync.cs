using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<IReadOnlyList<UserDto>>> GetUsersBulkAsync(
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