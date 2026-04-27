using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

using Response = Results<Ok<IReadOnlyList<UserDto>>, BadRequest<string>>;

public static partial class Api
{
    /// <summary>
    /// Получить список пользователей
    /// </summary>
    public static async Task<Response> GetUsersPagedAsync(
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