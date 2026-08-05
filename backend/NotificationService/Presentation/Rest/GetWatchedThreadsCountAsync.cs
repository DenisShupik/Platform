using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.ValueObjects;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить количество тем, на которые подписан текущий пользователь
    /// </summary>
    private static async Task<Ok<Count>> GetWatchedThreadsCountAsync(
        GetWatchedThreadsCountRequest request,
        [FromServices] GetWatchedThreadsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetWatchedThreadsCountQuery { QueriedBy = request.RequestedBy.UserId };
        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
