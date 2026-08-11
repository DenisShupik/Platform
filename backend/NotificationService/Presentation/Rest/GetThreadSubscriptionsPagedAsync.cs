using CoreService.Application.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.ValueObjects;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace NotificationService.Presentation.Rest;

using Response = Results<Ok<PagedList<ThreadDto>>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getThreadSubscriptionsPaged']/*" />
    public static async Task<Response> GetThreadSubscriptionsPagedAsync(
        GetThreadSubscriptionsPagedRequest request,
        [FromServices] GetThreadSubscriptionsPagedQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadSubscriptionsPagedQuery
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
