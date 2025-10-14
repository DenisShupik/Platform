using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<PostIndex>,
    NotFound<PostNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<ReadPolicyRestrictedError>
>;

public static partial class Api
{
    private static async Task<Response> GetPostIndexAsync(
        ClaimsPrincipal claimsPrincipal,
        GetPostIndexRequest request,
        [FromServices] GetPostIndexQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var command = new GetPostIndexQuery
        {
            PostId = request.PostId,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<ReadPolicyRestrictedError>(error)
        );
    }
}