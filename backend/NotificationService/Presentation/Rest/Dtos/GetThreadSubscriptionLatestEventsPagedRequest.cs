using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;
using Shared.Presentation.ValueObjects;
using NotificationService.Application.UseCases;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetThreadSubscriptionLatestEventsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly
            SortCriteria<GetThreadSubscriptionLatestEventsPagedQuerySortType> Sort =
            new()
            {
                Field = GetThreadSubscriptionLatestEventsPagedQuerySortType.LatestEvent,
                Order = SortOrderType.Descending
            };
    }

    [FromRoute] public required UserId UserId { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetThreadSubscriptionLatestEventsPagedQuerySortType> Sort { get; init; }
}
