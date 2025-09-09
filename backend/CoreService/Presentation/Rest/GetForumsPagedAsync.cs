using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<GetForumsPagedQueryResult>> GetForumsPagedAsync(
        GetForumsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsPagedQuery
        {
            Title = request.Title,
            CreatedBy = request.CreatedBy,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
        };

        var result = await messageBus.InvokeAsync<GetForumsPagedQueryResult>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}