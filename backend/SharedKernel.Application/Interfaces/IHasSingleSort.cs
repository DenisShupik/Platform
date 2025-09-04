using System;
using SharedKernel.Application.Abstractions;

namespace SharedKernel.Application.Interfaces;

public interface IHasSingleSort<T>
    where T : Enum
{
    public SortCriteria<T> Sort { get; }
}