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
public sealed partial class GetCategoriesPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteriaList<GetCategoriesPagedQuery.SortType> Sort =
        [
            new()
            {
                Field = GetCategoriesPagedQuery.SortType.CategoryId,
                Order = SortOrderType.Ascending
            }
        ];
    }

    [FromQuery] public required IdSet<ForumId>? ForumIds { get; init; }
    [FromQuery] public required CategoryTitle? Title { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteriaList<GetCategoriesPagedQuery.SortType> Sort { get; init; }
}