using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async
        Task<Results<Ok, NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>>>
        UpdatePostAsync(
            ClaimsPrincipal claimsPrincipal,
            UpdatePostRequest request,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new UpdatePostCommand
        {
            PostId = request.PostId,
            Content = request.Body.Content,
            RowVersion = request.Body.RowVersion,
            UpdateBy = userId
        };

        var result = await messageBus.InvokeAsync<UpdatePostCommandResult>(command, cancellationToken);

        return result
            .Match<Results<Ok, NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>>>(
                _ => TypedResults.Ok(),
                notFound => TypedResults.NotFound(notFound),
                nonPostAuthorError => new Forbid<NonPostAuthorError>(nonPostAuthorError),
                staleError => TypedResults.Conflict(staleError)
            );
    }
}