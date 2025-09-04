using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

public sealed class GetCategoryThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly SortCriteria<GetCategoryThreadsPagedQuery.SortType> Sort =
            new()
            {
                Field = GetCategoryThreadsPagedQuery.SortType.Activity,
                Order = SortOrderType.Ascending
            };
    }

    [FromRoute] public CategoryId CategoryId { get; set; }
    [FromQuery] public bool IncludeDraft { get; set; } = false;
    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;
    [FromQuery] public SortCriteria<GetCategoryThreadsPagedQuery.SortType> Sort { get; set; } = Defaults.Sort;
}