using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Interfaces;

public interface IPagination<TLimit>
    where TLimit : struct, IPaginationLimit, IVogen<TLimit, int>
{
    public PaginationOffset? Offset { get; }
    public TLimit? Limit { get; }
}