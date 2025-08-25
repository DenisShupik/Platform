using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Abstractions;

public abstract class PagedQuery<T> : IPagedQuery<T>
    where T : struct, IPaginationLimit, IVogen<T, int>
{
    public required PaginationOffset? Offset { get; init; }
    public required T? Limit { get; init; }
}