using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.ValueObjects;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список тем, на которые подписан текущий пользователь
    /// </summary>
    private static async Task<Ok<IReadOnlyList<WatchedThreadDto>>> GetWatchedThreadsPagedAsync(
        GetWatchedThreadsPagedRequest request,
        [FromServices] GetWatchedThreadsPagedQueryHandler<WatchedThreadDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetWatchedThreadsPagedQuery<WatchedThreadDto>
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
