using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.ValueObjects;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<InternalNotificationsPagedDto>> GetInternalNotificationsPagedAsync(
        GetInternalNotificationsPagedRequest request,
        [FromServices] GetInternalNotificationsPagedQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new GetInternalNotificationsPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            UserId = request.RequestedBy.UserId,
            IsDelivered = request.IsDelivered
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}