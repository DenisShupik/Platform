using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

using Response = Ok<Dictionary<CategoryId, PostDto>>;

public static partial class Api
{
    /// <summary>
    /// Получить последние сообщения в разделах по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetCategoriesPostsLatestAsync(
        GetCategoriesPostsLatestRequest request,
        [FromServices] GetCategoriesPostsLatestQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPostsLatestQuery<PostDto>
        {
            CategoryIds = request.CategoryIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
