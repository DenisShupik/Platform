using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Presentation.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<int>> GetInternalNotificationCountAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromQuery] bool? isDelivered,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationCountQuery
        {
            UserId = userId,
            IsDelivered = isDelivered
        };
        var result = await messageBus.InvokeAsync<int>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}