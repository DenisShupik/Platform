using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator.Attributes;
using Shared.Presentation.ValueObjects;
using NotificationService.Application.UseCases;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetWatchedThreadLatestEventPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly
            SortCriteria<GetWatchedThreadLatestEventPagedQuerySortType> Sort =
            new()
            {
                Field = GetWatchedThreadLatestEventPagedQuerySortType.LatestEvent,
                Order = SortOrderType.Descending
            };
    }

    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetWatchedThreadLatestEventPagedQuerySortType> Sort { get; init; }
}
