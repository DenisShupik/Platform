using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<GetThreadSubscriptionStatusQueryResult>> GetThreadSubscriptionStatusAsync(
        GetThreadSubscriptionStatusRequest request,
        [FromServices] GetThreadSubscriptionStatusQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new GetThreadSubscriptionStatusQuery
        {
            ThreadId = request.ThreadId,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}