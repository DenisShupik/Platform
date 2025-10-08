using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<ulong>> GetForumsCountAsync(
        ClaimsPrincipal claimsPrincipal,
        GetForumsCountRequest request,
        [FromServices] GetForumsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        var query = new GetForumsCountQuery
        {
            CreatedBy = request.CreatedBy,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}