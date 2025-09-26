using System;
using Shared.Application.Abstractions;

namespace Shared.Application.Interfaces;

public interface IHasSingleSort<T>
    where T : Enum
{
    public SortCriteria<T> Sort { get; }
}