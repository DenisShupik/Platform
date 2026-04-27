using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Presentation.Rest;

using Response =
    Ok<Dictionary<ForumId, Result<ForumDto, ForumNotFoundError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить форумы по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetForumsBulkAsync(
        GetForumsBulkRequest request,
        [FromServices] GetForumsBulkQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsBulkQuery<ForumDto>
        {
            ForumIds = request.ForumIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}