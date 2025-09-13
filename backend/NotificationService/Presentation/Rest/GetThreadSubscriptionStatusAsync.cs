using System.Security.Claims;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Presentation.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<GetThreadSubscriptionStatusQueryResult>> GetThreadSubscriptionStatusAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] ThreadId threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetThreadSubscriptionStatusQuery
        {
            UserId = userId,
            ThreadId = threadId,
        };
        var result = await messageBus.InvokeAsync<GetThreadSubscriptionStatusQueryResult>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}