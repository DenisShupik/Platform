using CoreService.Application.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.ValueObjects;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список тем, на которые подписан текущий пользователь
    /// </summary>
    private static async Task<Ok<PagedList<ThreadDto>>> GetWatchedThreadsPagedAsync(
        GetWatchedThreadsPagedRequest request,
        [FromServices] GetWatchedThreadsPagedQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetWatchedThreadsPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy.UserId
        };

        var result = await handler.HandleAsync(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
