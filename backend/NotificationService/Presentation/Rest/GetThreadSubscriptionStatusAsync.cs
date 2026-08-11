using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace NotificationService.Presentation.Rest;

using Response = Results<Ok<GetThreadSubscriptionStatusQueryResult>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getThreadSubscriptionStatus']/*" />
    public static async Task<Response> GetThreadSubscriptionStatusAsync(
        GetThreadSubscriptionStatusRequest request,
        [FromServices] GetThreadSubscriptionStatusQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadSubscriptionStatusQuery
        {
            ThreadId = request.ThreadId,
            UserId = request.UserId,
            RequestedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => new Forbid<NotAdminError>(error)
        );
    }
}
