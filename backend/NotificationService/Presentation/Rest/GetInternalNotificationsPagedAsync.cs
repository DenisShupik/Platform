using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.ValueObjects;
using Shared.Presentation.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<InternalNotificationsPagedDto>> GetInternalNotificationsPagedAsync(
        ClaimsPrincipal claimsPrincipal,
        GetInternalNotificationsPagedRequest request,
        [FromServices] GetInternalNotificationsPagedQueryHandler handler,
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

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}