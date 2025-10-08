using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<ThreadId, PostDto>>> GetThreadsPostsLatestAsync(
        ClaimsPrincipal claimsPrincipal,
        GetThreadsPostsLatestRequest request,
        [FromServices] GetThreadsPostsLatestQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsPostsLatestQuery<PostDto>
        {
            ThreadIds = request.ThreadIds,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}