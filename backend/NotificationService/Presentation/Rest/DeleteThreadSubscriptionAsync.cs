using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace NotificationService.Presentation.Rest;

using Response = Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='deleteThreadSubscription']/*" />
    public static async Task<Response> DeleteThreadSubscriptionAsync(
        DeleteThreadSubscriptionRequest request,
        [FromServices] DeleteThreadSubscriptionCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteThreadSubscriptionCommand
        {
            UserId = request.UserId,
            ThreadId = request.ThreadId,
            RequestedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            notFoundError => TypedResults.NotFound(notFoundError),
            error => new Forbid<NotAdminError>(error)
        );
    }
}
