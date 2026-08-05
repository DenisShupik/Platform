using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.ValueObjects;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace NotificationService.Presentation.Rest;

using Response = Results<Ok<IReadOnlyList<ThreadSubscriptionLatestEventDto>>, Forbid<NotAdminError>>;

public static partial class Api
{
    private static async Task<Response> GetThreadSubscriptionLatestEventsPagedAsync(
        GetThreadSubscriptionLatestEventsPagedRequest request,
        [FromServices] GetThreadSubscriptionLatestEventsPagedQueryHandler<ThreadSubscriptionLatestEventDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadSubscriptionLatestEventsPagedQuery<ThreadSubscriptionLatestEventDto>
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
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
