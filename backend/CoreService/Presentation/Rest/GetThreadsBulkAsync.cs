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
    Ok<Dictionary<ThreadId, Result<ThreadDto, ThreadNotFoundError, PermissionDeniedError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить темы по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetThreadsBulkAsync(
        GetThreadsBulkRequest request,
        [FromServices] GetThreadsBulkQueryHandler<ThreadDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsBulkQuery<ThreadDto>
        {
            ThreadIds = request.ThreadIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}