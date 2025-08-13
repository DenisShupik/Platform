namespace SharedKernel.Application.Abstractions;

public readonly struct PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required long TotalCount { get; init; }
}