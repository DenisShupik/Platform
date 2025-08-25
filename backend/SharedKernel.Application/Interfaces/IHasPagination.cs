using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Interfaces;

public interface IHasPagination<T>
    where T : struct, IPaginationLimit, IVogen<T, int>
{
    public PaginationOffset? Offset { get; }
    public T? Limit { get; }
}