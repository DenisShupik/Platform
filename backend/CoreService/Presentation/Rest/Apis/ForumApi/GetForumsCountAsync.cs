using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ForumApi
{
    private static async Task<Ok<Int64>> GetForumsCountAsync(
        [FromQuery] UserId? createdBy,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCountQuery
        {
            CreatedBy = createdBy
        };

        var result = await messageBus.InvokeAsync<Int64>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}