using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Generator;
using SharedKernel.Presentation.ValueObjects;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetInternalNotificationsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly
            SortCriteriaList<GetInternalNotificationsPagedQuery.SortType> Sort =
            [
                new()
                {
                    Field = GetInternalNotificationsPagedQuery.SortType.OccurredAt,
                    Order = SortOrderType.Ascending
                }
            ];
    }

    [FromQuery] public required bool? IsDelivered { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteriaList<GetInternalNotificationsPagedQuery.SortType> Sort { get; init; }
}