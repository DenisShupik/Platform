using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<NoContent, Conflict<DuplicateThreadSubscriptionError>>>
        CreateThreadSubscriptionAsync(
            CreateThreadSubscriptionRequest request,
            [FromServices] CreateThreadSubscriptionCommandHandler handler,
            CancellationToken cancellationToken
        )
    {
        var command = new CreateThreadSubscriptionCommand
        {
            UserId = request.RequestedBy.UserId,
            ThreadId = request.ThreadId,
            Channels = request.Body.Channels
        };
        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Results<NoContent, Conflict<DuplicateThreadSubscriptionError>>>(
                _ => TypedResults.NoContent(),
                duplicateError => TypedResults.Conflict(duplicateError)
            );
    }
}