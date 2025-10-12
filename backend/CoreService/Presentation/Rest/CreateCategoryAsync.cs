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
    Ok<CategoryId>,
    NotFound<ForumNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<AccessPolicyRestrictedError>,
    Forbid<CategoryCreatePolicyRestrictedError>
>;

public static partial class Api
{
    private static async Task<Response> CreateCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateCategoryRequestBody body,
        [FromServices] CreateCategoryCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var command = new CreateCategoryCommand
        {
            ForumId = body.ForumId,
            Title = body.Title,
            AccessPolicyValue = body.AccessPolicyValue,
            ThreadCreatePolicyValue = body.ThreadCreatePolicyValue,
            PostCreatePolicyValue= body.PostCreatePolicyValue,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            categoryId => TypedResults.Ok(categoryId),
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<AccessPolicyRestrictedError>(error),
            error => new Forbid<CategoryCreatePolicyRestrictedError>(error)
        );
    }
}