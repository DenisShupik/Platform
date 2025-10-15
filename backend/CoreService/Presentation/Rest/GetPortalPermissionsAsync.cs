using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Ok<Dictionary<PolicyType, bool>>;

public static partial class Api
{
    private static async Task<Response> GetPortalPermissionsAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromServices] GetPortalPermissionsQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        
        var query = new GetPortalPermissionsQuery
        {
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}