using SharedKernel.Application.Abstractions;

namespace SharedKernel.Application.Interfaces;

public interface IHasSortList<T>
    where T : Enum
{
    public SortCriteriaList<T>? Sort { get; }
}