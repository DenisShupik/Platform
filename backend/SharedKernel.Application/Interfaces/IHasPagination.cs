using SharedKernel.Application.ValueObjects;

namespace SharedKernel.Application.Interfaces;

public interface IHasPagination
{
    public PaginationOffset Offset { get; }
    public PaginationLimit Limit { get; }
}