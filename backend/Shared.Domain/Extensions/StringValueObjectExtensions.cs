using Shared.Domain.Interfaces;

namespace Shared.Domain.Extensions;

public static class StringValueObjectExtensions
{
    public static bool Contains<TValueObject>(
        this TValueObject source,
        TValueObject value,
        StringComparison comparisonType)
        where TValueObject : struct, IVogen<TValueObject, string> =>
        source.Value.Contains(value.Value, comparisonType);
}
