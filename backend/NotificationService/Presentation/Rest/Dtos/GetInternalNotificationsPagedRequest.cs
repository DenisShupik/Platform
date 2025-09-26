using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator;
using Shared.Presentation.ValueObjects;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetInternalNotificationsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly
            SortCriteriaList<GetInternalNotificationsPagedQuerySortType> Sort =
            [
                new()
                {
                    Field = GetInternalNotificationsPagedQuerySortType.OccurredAt,
                    Order = SortOrderType.Ascending
                }
            ];
    }

    [FromQuery] public required bool? IsDelivered { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteriaList<GetInternalNotificationsPagedQuerySortType> Sort { get; init; }
}