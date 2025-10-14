using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;
using UserService.Domain.Enums;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<ThreadDto>, 
    NotFound<ThreadNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<ReadPolicyRestrictedError>,
    Forbid<NonThreadOwnerError>
>;

public static partial class Api
{
    private static async Task<Response>
        GetThreadAsync(
            ClaimsPrincipal claimsPrincipal,
            GetThreadRequest  request,
            [FromServices] GetThreadQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadQuery<ThreadDto>
        {
            ThreadId = request.ThreadId,
            Role = RoleType.User,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<ReadPolicyRestrictedError>(error),
            error => new Forbid<NonThreadOwnerError>(error)
        );
    }
}