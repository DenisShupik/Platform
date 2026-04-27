using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<NoContent, NotFound<NotificationNotFoundError>>>
        DeleteInternalNotificationAsync(
            DeleteInternalNotificationRequest request,
            [FromServices] DeleteInternalNotificationCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var command = new DeleteInternalNotificationCommand
        {
            UserId = request.RequestedBy.UserId,
            NotifiableEventId = request.NotifiableEventId
        };
        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Results<NoContent, NotFound<NotificationNotFoundError>>>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}