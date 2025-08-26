using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Abstractions;

public abstract class PagedQuery<TPagination, TSort> : IHasPagination<TPagination>, IHasSort<TSort>
    where TPagination : struct, IPaginationLimit, IVogen<TPagination, int> 
    where TSort : Enum
{
    public required PaginationOffset? Offset { get; init; }
    public required TPagination? Limit { get; init; }
    public required SortCriteriaList<TSort>? Sort { get; init; }
}