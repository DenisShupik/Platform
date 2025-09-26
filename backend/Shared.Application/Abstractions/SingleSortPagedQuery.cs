using System;
using Shared.Application.Interfaces;
using Shared.Application.ValueObjects;

namespace Shared.Application.Abstractions;

public abstract class SingleSortPagedQuery<TResult, TSort> :
    IQuery<TResult>, IHasPagination, IHasSingleSort<TSort>
    where TSort : Enum
{
    public required PaginationOffset Offset { get; init; }
    public required PaginationLimit Limit { get; init; }
    public required SortCriteria<TSort> Sort { get; init; }
}