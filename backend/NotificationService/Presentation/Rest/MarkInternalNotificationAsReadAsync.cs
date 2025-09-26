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
    private static async Task<Results<NoContent, NotFound<NotificationNotFoundError>>>
        MarkInternalNotificationAsReadAsync(
            ClaimsPrincipal claimsPrincipal,
            MarkInternalNotificationAsReadRequest request,
            [FromServices] MarkInternalNotificationAsReadCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new MarkInternalNotificationAsReadCommand
        {
            UserId = userId,
            NotifiableEventId = request.NotifiableEventId
        };
        var result =
            await handler.HandleAsync(command, cancellationToken);

        return result.Match<Results<NoContent, NotFound<NotificationNotFoundError>>>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}