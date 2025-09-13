using Shared.Application.ValueObjects;

namespace Shared.Application.Interfaces;

public interface IHasPagination
{
    public PaginationOffset Offset { get; }
    public PaginationLimit Limit { get; }
}