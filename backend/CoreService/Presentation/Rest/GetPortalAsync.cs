using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

using Response = Ok<PortalDto>;

public static partial class Api
{
    private static async Task<Response> GetPortalAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromServices] GetPortalQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetPortalQuery();

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}