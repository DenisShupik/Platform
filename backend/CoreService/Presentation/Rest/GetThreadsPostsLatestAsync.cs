using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<ThreadId, PostDto>>> GetThreadsPostsLatestAsync(
        GetThreadsPostsLatestRequest request,
        [FromServices] GetThreadsPostsLatestQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsLatestQuery<PostDto>
        {
            ThreadIds = request.ThreadIds
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}