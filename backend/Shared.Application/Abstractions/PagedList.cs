using System.Collections.Generic;
using Shared.Domain.ValueObjects;

namespace Shared.Application.Abstractions;

public readonly struct PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required Count TotalCount { get; init; }
}