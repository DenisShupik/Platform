using SharedKernel.Application.Abstractions;

namespace SharedKernel.Application.Interfaces;

public interface IHasSort<T>
    where T : Enum
{
    public SortCriteriaList<T>? Sort { get; }
}