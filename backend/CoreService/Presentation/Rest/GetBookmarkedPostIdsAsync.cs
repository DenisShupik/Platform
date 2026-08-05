using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить идентификаторы сообщений из набора, добавленных текущим пользователем в закладки
    /// </summary>
    public static async Task<Ok<GetBookmarkedPostIdsResponse>> GetBookmarkedPostIdsAsync(
        GetBookmarkedPostIdsRequest request,
        [FromServices] GetBookmarkedPostIdsQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookmarkedPostIdsQuery
        {
            QueriedBy = request.RequestedBy.UserId,
            PostIds = request.PostIds
        };

        var bookmarkedPostIds = await handler.HandleAsync(query, cancellationToken);
        return TypedResults.Ok(new GetBookmarkedPostIdsResponse { PostIds = bookmarkedPostIds.ToArray() });
    }
}
