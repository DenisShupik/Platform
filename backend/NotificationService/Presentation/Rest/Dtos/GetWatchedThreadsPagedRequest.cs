using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator.Attributes;
using Shared.Presentation.ValueObjects;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetWatchedThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetWatchedThreadsPagedQuerySortType> Sort =
            new()
            {
                Field = GetWatchedThreadsPagedQuerySortType.ThreadId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetWatchedThreadsPagedQuerySortType> Sort { get; init; }
}
