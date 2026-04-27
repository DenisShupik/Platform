using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response = Ok<Dictionary<ForumId, Result<Count, ForumNotFoundError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить количество разделов в форумах по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetForumsCategoriesCountAsync(
        GetForumsCategoriesCountRequest request,
        [FromServices] GetForumsCategoriesCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCategoriesCountQuery
        {
            ForumIds = request.ForumIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}