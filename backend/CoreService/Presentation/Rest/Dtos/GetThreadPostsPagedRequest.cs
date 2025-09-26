using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator;
using Shared.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadPostsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetThreadPostsPagedQuerySortType> Sort =
            new()
            {
                Field = GetThreadPostsPagedQuerySortType.Index,
                Order = SortOrderType.Ascending
            };
    }

    [FromRoute] public required ThreadId ThreadId { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetThreadPostsPagedQuerySortType> Sort { get; init; }
}