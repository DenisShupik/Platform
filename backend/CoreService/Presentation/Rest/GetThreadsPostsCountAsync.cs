using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<ThreadId, ulong>>> GetThreadsPostsCountAsync(
        GetThreadsPostsCountRequest request,
        [FromServices] GetThreadsPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsCountQuery
        {
            ThreadIds = request.ThreadIds
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}