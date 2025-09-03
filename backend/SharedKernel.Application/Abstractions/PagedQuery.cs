using System;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Abstractions;

public abstract class PagedQuery<TSort> : IHasPagination, IHasSort<TSort>
    where TSort : Enum
{
    public required PaginationOffset Offset { get; init; }
    public required PaginationLimit Limit { get; init; }
    public required SortCriteriaList<TSort> Sort { get; init; }
}