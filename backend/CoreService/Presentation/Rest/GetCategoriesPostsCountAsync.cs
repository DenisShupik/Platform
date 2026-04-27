using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response =
    Ok<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить количество сообщений в разделах по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetCategoriesPostsCountAsync(
        GetCategoriesPostsCountRequest request,
        [FromServices] GetCategoriesPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPostsCountQuery
        {
            CategoryIds = request.CategoryIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}