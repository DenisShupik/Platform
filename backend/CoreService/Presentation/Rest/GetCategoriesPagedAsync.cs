using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<GetCategoriesPagedQueryResult>> GetCategoriesPagedAsync(
        GetCategoriesPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPagedQuery
        {
            ForumIds = request.ForumIds,
            Title = request.Title,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
        };

        var result = await messageBus.InvokeAsync<GetCategoriesPagedQueryResult>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}