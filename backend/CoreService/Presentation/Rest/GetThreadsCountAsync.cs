using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<long>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromQuery] UserId? createdBy,
            [FromQuery] ThreadStatus? status,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsCountQuery
        {
            CreatedBy = createdBy,
            Status = status,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadsCountQueryResult>(query, cancellationToken);

        return result.Match<Results<Ok<long>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            count => TypedResults.Ok(count),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }
}