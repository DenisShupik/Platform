using System.Collections.Generic;

namespace Shared.Application.Abstractions;

public readonly struct PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required ulong TotalCount { get; init; }
}