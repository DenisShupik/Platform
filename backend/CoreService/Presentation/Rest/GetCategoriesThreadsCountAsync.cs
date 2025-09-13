using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<CategoryId, long>>> GetCategoriesThreadsCountAsync(
        [FromRoute] IdSet<CategoryId> categoryIds,
        [FromQuery] bool? includeDraft,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesThreadsCountQuery
        {
            CategoryIds = categoryIds,
            IncludeDraft = includeDraft ?? false
        };

        var result = await messageBus.InvokeAsync<Dictionary<CategoryId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}