using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<InternalNotificationsPagedDto>> GetInternalNotificationsPagedAsync(
        ClaimsPrincipal claimsPrincipal,
        GetInternalNotificationsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationsPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            UserId = userId,
            IsDelivered = request.IsDelivered
        };

        var result = await messageBus.InvokeAsync<InternalNotificationsPagedDto>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}