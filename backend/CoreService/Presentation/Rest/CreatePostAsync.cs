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

public static partial class Api
{
    private static async Task<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<AccessRestrictedError>,
            Forbid<NonThreadOwnerError>>>
        CreatePostAsync(
            ClaimsPrincipal claimsPrincipal,
            CreatePostRequest request,
            [FromServices] CreatePostCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreatePostCommand
        {
            ThreadId = request.ThreadId,
            Content = request.Body.Content,
            CreatedBy = userId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<AccessRestrictedError>,
                Forbid<NonThreadOwnerError>>>(
                postId => TypedResults.Ok(postId),
                notFound => TypedResults.NotFound(notFound),
                accessRestrictedError => new Forbid<AccessRestrictedError>(accessRestrictedError),
                nonThreadAuthor => new Forbid<NonThreadOwnerError>(nonThreadAuthor)
            );
    }
}