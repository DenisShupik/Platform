using CoreService.Application.Enums;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator;
using Shared.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetActivitiesPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteriaList<GetActivitiesPagedQuery.SortType> Sort =
        [
            new()
            {
                Field = GetActivitiesPagedQuery.SortType.Latest,
                Order = SortOrderType.Ascending
            }
        ];
    }

    [FromQuery] public required ActivityType Activity { get; init; }
    [FromQuery] public required GetActivitiesPagedQuery.GetActivitiesPagedQueryGroupByType GroupBy { get; init; }
    [FromQuery] public required GetActivitiesPagedQuery.GetActivitiesPagedQueryModeType Mode { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteriaList<GetActivitiesPagedQuery.SortType> Sort { get; init; }
}