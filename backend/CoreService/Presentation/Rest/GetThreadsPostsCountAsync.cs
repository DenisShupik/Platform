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
    Ok<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить количество сообщений в темах по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetThreadsPostsCountAsync(
        GetThreadsPostsCountRequest request,
        [FromServices] GetThreadsPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsCountQuery
        {
            ThreadIds = request.ThreadIds,
            Status = request.Status,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}