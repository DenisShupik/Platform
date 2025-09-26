using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Presentation.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<GetThreadSubscriptionStatusQueryResult>> GetThreadSubscriptionStatusAsync(
        ClaimsPrincipal claimsPrincipal,
        GetThreadSubscriptionStatusRequest request,
        [FromServices] GetThreadSubscriptionStatusQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetThreadSubscriptionStatusQuery
        {
            UserId = userId,
            ThreadId = request.ThreadId,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}