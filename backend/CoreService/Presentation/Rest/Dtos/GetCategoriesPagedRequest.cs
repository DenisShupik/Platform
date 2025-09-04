using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

public sealed class GetCategoriesPagedRequest
{
    private static class Defaults
    {
        public static readonly SortCriteriaList<GetCategoriesPagedQuery.SortType> Sort =
        [
            new()
            {
                Field = GetCategoriesPagedQuery.SortType.CategoryId,
                Order = SortOrderType.Ascending
            }
        ];
    }

    [FromQuery] public IdSet<ForumId>? ForumIds { get; set; }
    [FromQuery] public CategoryTitle? Title { get; set; }
    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

    [FromQuery]
    public SortCriteriaList<GetCategoriesPagedQuery.SortType> Sort { get; set; } =
        Defaults.Sort;
}