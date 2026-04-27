using System;
using Shared.Application.Abstractions;

namespace Shared.Application.Interfaces;

public interface IHasMultiSort<T>
    where T : Enum
{
    public SortCriteriaList<T> Sort { get; }
}