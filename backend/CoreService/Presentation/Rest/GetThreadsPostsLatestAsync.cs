using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<ThreadId, PostDto>>> GetThreadsPostsLatestAsync(
        [FromRoute] IdSet<ThreadId> threadIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsLatestQuery
        {
            ThreadIds = threadIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<ThreadId, PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}