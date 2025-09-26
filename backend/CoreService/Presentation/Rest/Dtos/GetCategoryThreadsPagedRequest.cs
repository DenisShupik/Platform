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
public sealed partial class GetCategoryThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly bool IncludeDraft = false;
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetCategoryThreadsPagedQuerySortType> Sort =
            new()
            {
                Field = GetCategoryThreadsPagedQuerySortType.Activity,
                Order = SortOrderType.Ascending
            };
    }

    [FromRoute] public required CategoryId CategoryId { get; init; }
    [FromQuery] public required bool IncludeDraft { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetCategoryThreadsPagedQuerySortType> Sort { get; init; }
}