using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Presentation.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    // TODO: сделать requset класс
    private static async Task<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>
        DeleteThreadSubscriptionAsync(
            ClaimsPrincipal claimsPrincipal,
            DeleteThreadSubscriptionRequest request,
            [FromServices] DeleteThreadSubscriptionCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new DeleteThreadSubscriptionCommand
        {
            UserId = userId,
            ThreadId = request.ThreadId,
        };
        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>(
                _ => TypedResults.NoContent(),
                notFoundError => TypedResults.NotFound(notFoundError)
            );
    }
}