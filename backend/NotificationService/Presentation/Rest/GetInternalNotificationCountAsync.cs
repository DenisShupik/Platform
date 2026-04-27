using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.ValueObjects;

namespace NotificationService.Presentation.Rest;

using Response = Ok<Count>;

public static partial class Api
{
    private static async Task<Response> GetInternalNotificationCountAsync(
        GetInternalNotificationCountRequest request,
        [FromServices] GetInternalNotificationCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new GetInternalNotificationCountQuery
        {
            IsDelivered = request.IsDelivered,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}