using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace UserService.Presentation.Rest;

public static partial class Api
{
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