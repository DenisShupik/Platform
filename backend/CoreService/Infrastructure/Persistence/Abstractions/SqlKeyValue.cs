namespace CoreService.Infrastructure.Persistence.Abstractions;

public sealed class SqlKeyValue<TKey, TValue>
{
    public required TKey Key { get; init; }
    public required TValue Value { get; init; }
}