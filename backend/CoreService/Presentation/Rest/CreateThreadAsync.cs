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
    Forbid<ForumAccessPolicyViolationError>,
    Forbid<ForumPolicyRestrictedError>,
    Forbid<CategoryAccessPolicyViolationError>,
    Forbid<CategoryPolicyRestrictedError>,
    Forbid<ThreadCreatePolicyViolationError>
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
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateThreadCommand
        {
            CategoryId = body.CategoryId,
            Title = body.Title,
            ThreadPolicySetId = body.ThreadPolicySetId,
            CreatedBy = userId,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<ForumAccessPolicyViolationError>(error),
            error => new Forbid<ForumPolicyRestrictedError>(error),
            error => new Forbid<CategoryAccessPolicyViolationError>(error),
            error => new Forbid<CategoryPolicyRestrictedError>(error),
            error => new Forbid<ThreadCreatePolicyViolationError>(error)
        );
    }
}