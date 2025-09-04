using System;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Abstractions;

public abstract class MultiSortPagedQuery<TSort> : IHasPagination, IHasMultiSort<TSort>
    where TSort : Enum
{
    public required PaginationOffset Offset { get; init; }
    public required PaginationLimit Limit { get; init; }
    public required SortCriteriaList<TSort> Sort { get; init; }
}