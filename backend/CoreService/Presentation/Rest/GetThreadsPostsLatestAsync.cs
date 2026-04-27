using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить последние сообщения в темах по списку идентификаторов
    /// </summary>
    public static async
        Task<Ok<Dictionary<ThreadId, Result<PostDto, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>>
        GetThreadsPostsLatestAsync(
            GetThreadsPostsLatestRequest request,
            [FromServices] GetThreadsPostsLatestQueryHandler<PostDto> handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetThreadsPostsLatestQuery<PostDto>
        {
            ThreadIds = request.ThreadIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}