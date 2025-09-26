using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<IReadOnlyList<PostDto>>, NotFound<ThreadNotFoundError>>>
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
            Sort = request.Sort
        };

        var result = await handler.HandleAsync(query, cancellationToken);
        
        return result.Match<Results<Ok<IReadOnlyList<PostDto>>, NotFound<ThreadNotFoundError>>>(
            value => TypedResults.Ok(value),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}