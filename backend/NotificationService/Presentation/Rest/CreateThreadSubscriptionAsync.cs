using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace NotificationService.Presentation.Rest;

using Response = Results<NoContent, Conflict<DuplicateThreadSubscriptionError>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='createThreadSubscription']/*" />
    public static async Task<Response> CreateThreadSubscriptionAsync(
        CreateThreadSubscriptionRequest request,
        [FromServices] CreateThreadSubscriptionCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateThreadSubscriptionCommand
        {
            UserId = request.UserId,
            ThreadId = request.ThreadId,
            Channels = request.Body.Channels,
            RequestedBy = request.RequestedBy
        };
        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            duplicateError => TypedResults.Conflict(duplicateError),
            error => new Forbid<NotAdminError>(error)
        );
    }
}
