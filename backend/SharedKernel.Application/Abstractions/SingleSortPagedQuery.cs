using System;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Abstractions;

public abstract class SingleSortPagedQuery<TSort> : IHasPagination, IHasSingleSort<TSort>
    where TSort : Enum
{
    public required PaginationOffset Offset { get; init; }
    public required PaginationLimit Limit { get; init; }
    public required SortCriteria<TSort> Sort { get; init; }
}