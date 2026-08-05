using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список сообщений, добавленных текущим пользователем в закладки
    /// </summary>
    public static async Task<Ok<IReadOnlyList<PostDto>>> GetBookmarkedPostsPagedAsync(
        GetBookmarkedPostsPagedRequest request,
        [FromServices] GetBookmarkedPostsPagedQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookmarkedPostsPagedQuery<PostDto>
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
