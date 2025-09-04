using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;
using Wolverine;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<IReadOnlyList<UserDto>>, BadRequest<string>>> GetUsersPagedAsync(
        [AsParameters] GetUsersPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}