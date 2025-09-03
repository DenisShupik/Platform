using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Presentation.Extensions;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>
        CreatePostAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromBody] CreatePostRequestBody body,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreatePostCommand
        {
            ThreadId = threadId,
            Content = body.Content,
            CreatedBy = userId
        };

        var result = await messageBus.InvokeAsync<CreatePostCommandResult>(command, cancellationToken);

        return result.Match<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            postId => TypedResults.Ok(postId),
            notFound => TypedResults.NotFound(notFound),
            nonThreadAuthor => new Forbid<NonThreadOwnerError>(nonThreadAuthor)
        );
    }
}