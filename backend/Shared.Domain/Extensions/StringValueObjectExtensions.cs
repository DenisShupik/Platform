namespace Shared.Domain.Extensions;

public static class StringValueObjectExtensions
{
    public static bool Contains<TValueObject>(
        this TValueObject source,
        TValueObject value,
        StringComparison comparisonType)
        where TValueObject : struct, IVogen<TValueObject, string> =>
        source.Value.Contains(value.Value, comparisonType);

    public static bool Contains<TValueObject>(
        this TValueObject source,
        string value,
        StringComparison comparisonType)
        where TValueObject : struct, IVogen<TValueObject, string> =>
        source.Value.Contains(value, comparisonType);
}
