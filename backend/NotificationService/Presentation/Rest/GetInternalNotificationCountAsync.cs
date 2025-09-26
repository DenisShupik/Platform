using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Presentation.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<ulong>> GetInternalNotificationCountAsync(
        ClaimsPrincipal claimsPrincipal,
        GetInternalNotificationCountRequest request,
        [FromServices] GetInternalNotificationCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationCountQuery
        {
            UserId = userId,
            IsDelivered = request.IsDelivered
        };
        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}