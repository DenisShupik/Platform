using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список сообщений темы
    /// </summary>
    public static async Task<Results<Ok<IReadOnlyList<PostDto>>, NotFound<ThreadNotFoundError>, Forbid<PermissionDeniedError>>>
        GetThreadPostsPagedAsync(
            GetThreadPostsPagedRequest request,
            [FromServices] GetThreadPostsPagedQueryHandler<PostDto> handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetThreadPostsPagedQuery<PostDto>
        {
            ThreadId = request.ThreadId,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<IReadOnlyList<PostDto>>, NotFound<ThreadNotFoundError>, Forbid<PermissionDeniedError>>>(
            value => TypedResults.Ok(value),
            notFound => TypedResults.NotFound(notFound),
            error => new Forbid<PermissionDeniedError>(error)
        );
    }
}
