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
    Ok<PostId>,
    NotFound<ThreadNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<ReadPolicyRestrictedError>,
    Forbid<PostCreatePolicyRestrictedError>,
    Forbid<NonThreadOwnerError>
>;

public static partial class Api
{
    private static async Task<Response> CreatePostAsync(
        ClaimsPrincipal claimsPrincipal,
        CreatePostRequest request,
        [FromServices] CreatePostCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var command = new CreatePostCommand
        {
            ThreadId = request.ThreadId,
            Content = request.Body.Content,
            CreatedBy = userId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Response>(
                postId => TypedResults.Ok(postId),
                error => TypedResults.NotFound(error),
                error => new Forbid<PolicyViolationError>(error),
                error => new Forbid<ReadPolicyRestrictedError>(error),
                error => new Forbid<PostCreatePolicyRestrictedError>(error),
                error => new Forbid<NonThreadOwnerError>(error)
            );
    }
}