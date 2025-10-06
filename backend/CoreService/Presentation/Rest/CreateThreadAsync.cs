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
    Ok<ThreadId>,
    NotFound<CategoryNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<AccessPolicyRestrictedError>,
    Forbid<ThreadCreatePolicyRestrictedError>
>;

public static partial class Api
{
    private static async Task<Response> CreateThreadAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateThreadRequestBody body,
        [FromServices] CreateThreadCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var command = new CreateThreadCommand
        {
            CategoryId = body.CategoryId,
            Title = body.Title,
            AccessPolicyId = body.AccessPolicyId,
            PostCreatePolicyId = body.PostCreatePolicyId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<AccessPolicyRestrictedError>(error),
            error => new Forbid<ThreadCreatePolicyRestrictedError>(error)
        );
    }
}