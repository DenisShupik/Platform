using CoreService.Application.Dtos;
using CoreService.Application.Enums;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static class ActivityApi
{
    public static IEndpointRouteBuilder MapActivityApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/activities")
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetActivitiesPagedAsync);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<ActivityDto>>> GetActivitiesPagedAsync(
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] ActivityType activity,
        [FromQuery] GetActivitiesPagedQuery.GetActivitiesPagedQueryGroupByType groupBy,
        [FromQuery] GetActivitiesPagedQuery.GetActivitiesPagedQueryModeType mode,
        [FromQuery] SortCriteriaList<GetActivitiesPagedQuery.GetActivitiesPagedQuerySortType>? sort,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetActivitiesPagedQuery
        {
            Offset = offset,
            Limit = limit,
            Activity = activity,
            GetActivitiesPagedQueryGroupBy = groupBy,
            GetActivitiesPagedQueryMode = mode,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ActivityDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}