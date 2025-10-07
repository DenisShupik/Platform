using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<ForumId, ulong>>> GetForumsCategoriesCountAsync(
        ClaimsPrincipal claimsPrincipal,
        GetForumsCategoriesCountRequest request,
        [FromServices] GetForumsCategoriesCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        var query = new GetForumsCategoriesCountQuery
        {
            ForumIds = request.ForumIds,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}