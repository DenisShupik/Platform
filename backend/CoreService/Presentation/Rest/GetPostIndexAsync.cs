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
    Forbid<AccessLevelError>,
    Forbid<AccessRestrictedError>,
    NotFound<PostNotFoundError>
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
            order => TypedResults.Ok(order),
            accessLevelError => new Forbid<AccessLevelError>(accessLevelError),
            accessRestrictedError => new Forbid<AccessRestrictedError>(accessRestrictedError),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}