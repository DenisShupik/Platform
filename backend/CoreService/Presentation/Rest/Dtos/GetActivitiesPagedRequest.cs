using CoreService.Application.Enums;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

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