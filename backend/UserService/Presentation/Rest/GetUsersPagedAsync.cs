using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<IReadOnlyList<UserDto>>, BadRequest<string>>> GetUsersPagedAsync(
        GetUsersPagedRequest request,
        [FromServices] GetUsersPagedQueryHandler<UserDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersPagedQuery<UserDto>
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}