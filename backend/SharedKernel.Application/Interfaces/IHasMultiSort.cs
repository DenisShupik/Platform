using System;
using SharedKernel.Application.Abstractions;

namespace SharedKernel.Application.Interfaces;

public interface IHasMultiSort<T>
    where T : Enum
{
    public SortCriteriaList<T> Sort { get; }
}