using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ForumApi
{
    private static async Task<Ok<Dictionary<ForumId, long>>> GetForumsCategoriesCountAsync(
        [FromRoute] IdSet<ForumId> forumIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCategoriesCountQuery
        {
            ForumIds = forumIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<ForumId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}