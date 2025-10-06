using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<ForumDto>,
    Forbid<PolicyViolationError>,
    Forbid<AccessPolicyRestrictedError>,
    NotFound<ForumNotFoundError>
>;

public static partial class Api
{
    private static async Task<Response> GetForumAsync(
        ClaimsPrincipal claimsPrincipal,
        GetForumRequest request,
        [FromServices] GetForumQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetForumQuery<ForumDto>
        {
            ForumId = request.ForumId,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            forumDto => TypedResults.Ok(forumDto),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<AccessPolicyRestrictedError>(error),
            error => TypedResults.NotFound(error)
        );
    }
}