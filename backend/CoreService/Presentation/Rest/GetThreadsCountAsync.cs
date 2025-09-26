using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<ulong>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsCountAsync(
            ClaimsPrincipal claimsPrincipal,
            GetThreadsCountRequest request,
            [FromServices] GetThreadsCountQueryHandler handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsCountQuery
        {
            CreatedBy = request.CreatedBy,
            Status = request.Status,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<ulong>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            count => TypedResults.Ok(count),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }
}