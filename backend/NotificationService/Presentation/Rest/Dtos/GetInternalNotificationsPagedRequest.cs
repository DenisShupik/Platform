using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;

namespace NotificationService.Presentation.Rest.Dtos;

public sealed class GetInternalNotificationsPagedRequest
{
    private static class Defaults
    {
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

    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

    [FromQuery]
    public SortCriteriaList<GetInternalNotificationsPagedQuery.SortType> Sort
    {
        get;
        set;
    } = Defaults.Sort;

    [FromQuery] public bool? IsDelivered { get; set; }
}