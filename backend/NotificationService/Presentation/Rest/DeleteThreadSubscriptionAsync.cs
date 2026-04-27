using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>
        DeleteThreadSubscriptionAsync(
            DeleteThreadSubscriptionRequest request,
            [FromServices] DeleteThreadSubscriptionCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var command = new DeleteThreadSubscriptionCommand
        {
            UserId = request.RequestedBy.UserId,
            ThreadId = request.ThreadId
        };
        
        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>(
                _ => TypedResults.NoContent(),
                notFoundError => TypedResults.NotFound(notFoundError)
            );
    }
}