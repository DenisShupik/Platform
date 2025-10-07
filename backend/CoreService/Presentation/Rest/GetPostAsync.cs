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
    Ok<PostDto>,
    NotFound<PostNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<PolicyRestrictedError>
>;

public static partial class Api
{
    private static async Task<Response> GetPostAsync(
        ClaimsPrincipal claimsPrincipal,
        GetPostRequest request,
        [FromServices] GetPostQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetPostQuery<PostDto>
        {
            PostId = request.PostId,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<PolicyRestrictedError>(error)
        );
    }
}