using CoreService.Application.Dtos;
using CoreService.Application.Enums;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

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

    public sealed class GetActivitiesPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteriaList<GetActivitiesPagedQuery.SortType> Sort =
            [
                new()
                {
                    Field = GetActivitiesPagedQuery.SortType.Latest,
                    Order = SortOrderType.Ascending
                }
            ];
        }

        [FromQuery] public ActivityType Activity { get; set; }
        [FromQuery] public GetActivitiesPagedQuery.GetActivitiesPagedQueryGroupByType GroupBy { get; set; }
        [FromQuery] public GetActivitiesPagedQuery.GetActivitiesPagedQueryModeType Mode { get; set; }
        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetActivitiesPagedQuery.SortType> Sort { get; set; } =
            Defaults.Sort;
    }

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