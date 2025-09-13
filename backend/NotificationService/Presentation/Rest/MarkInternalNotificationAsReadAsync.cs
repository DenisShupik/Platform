using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using Shared.Presentation.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<NoContent, NotFound<NotificationNotFoundError>>>
        MarkInternalNotificationAsReadAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] NotifiableEventId notifiableEventId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new MarkInternalNotificationAsReadCommand
        {
            UserId = userId,
            NotifiableEventId = notifiableEventId
        };
        var result =
            await messageBus.InvokeAsync<MarkInternalNotificationAsReadCommandResult>(command, cancellationToken);

        return result.Match<Results<NoContent, NotFound<NotificationNotFoundError>>>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}