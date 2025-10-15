using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<Dictionary<PolicyType, bool>>,
    NotFound<ForumNotFoundError>
>;

public static partial class Api
{
    private static async Task<Response> GetForumPermissionsAsync(
        ClaimsPrincipal claimsPrincipal,
        GetForumPermissionsRequest request,
        [FromServices] GetForumPermissionsQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();

        var query = new GetForumPermissionsQuery
        {
            ForumId = request.ForumId,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error)
        );
    }
}