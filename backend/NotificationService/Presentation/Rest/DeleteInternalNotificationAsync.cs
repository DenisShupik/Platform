using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;

namespace NotificationService.Presentation.Rest;

using Response = Results<NoContent, NotFound<NotificationNotFoundError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='deleteInternalNotification']/*" />
    public static async Task<Response> DeleteInternalNotificationAsync(
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

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}
