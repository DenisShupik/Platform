using System.Security.Claims;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using Shared.Presentation.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>
        DeleteThreadSubscriptionAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new DeleteThreadSubscriptionCommand
        {
            UserId = userId,
            ThreadId = threadId,
        };
        var result = await messageBus.InvokeAsync<DeleteThreadSubscriptionCommandResult>(command, cancellationToken);

        return result
            .Match<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>(
                _ => TypedResults.NoContent(),
                notFoundError => TypedResults.NotFound(notFoundError)
            );
    }
}