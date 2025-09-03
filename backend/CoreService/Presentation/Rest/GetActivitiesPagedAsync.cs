using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<IReadOnlyList<ActivityDto>>> GetActivitiesPagedAsync(
        [AsParameters] GetActivitiesPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetActivitiesPagedQuery
        {
            Activity = request.Activity,
            GetActivitiesPagedQueryGroupBy = request.GroupBy,
            GetActivitiesPagedQueryMode = request.Mode,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ActivityDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}