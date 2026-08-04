using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class SearchRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.From(20);
        public static readonly SortCriteria<SearchQuerySortType> Sort = new()
        {
            Field = SearchQuerySortType.Relevance,
            Order = SortOrderType.Descending
        };
    }

    [FromQuery] public required SearchTerm Term { get; init; }
    [FromQuery] public required SearchResultType? Type { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required SortCriteria<SearchQuerySortType> Sort { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SearchCursor? Cursor { get; init; }
}
