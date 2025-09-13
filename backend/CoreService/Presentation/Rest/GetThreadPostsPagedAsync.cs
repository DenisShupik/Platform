using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<IReadOnlyList<PostDto>>> GetThreadPostsPagedAsync(
        GetThreadPostsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadPostsPagedQuery
        {
            ThreadId = request.ThreadId,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}