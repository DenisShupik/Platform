using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить количество доступных текущему пользователю сообщений в закладках
    /// </summary>
    public static async Task<Ok<Count>> GetBookmarkedPostsCountAsync(
        GetBookmarkedPostsCountRequest request,
        [FromServices] GetBookmarkedPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookmarkedPostsCountQuery { QueriedBy = request.RequestedBy };
        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
