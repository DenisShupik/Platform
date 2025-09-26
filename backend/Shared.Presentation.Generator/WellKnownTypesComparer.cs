using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Shared.Presentation.Generator;

public sealed class WellKnownTypesComparer : IEqualityComparer<WellKnownTypes>
{
    public static readonly WellKnownTypesComparer Instance = new();

    public bool Equals(WellKnownTypes? x, WellKnownTypes? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return SymbolEqualityComparer.Default.Equals(x.FromRouteAttribute, y.FromRouteAttribute) &&
               SymbolEqualityComparer.Default.Equals(x.FromBodyAttribute, y.FromBodyAttribute) &&
               SymbolEqualityComparer.Default.Equals(x.FromQueryAttribute, y.FromQueryAttribute) &&
               SymbolEqualityComparer.Default.Equals(x.IVogenInterface, y.IVogenInterface) &&
               SymbolEqualityComparer.Default.Equals(x.IValueTypeWithTryParseExtended,
                   y.IValueTypeWithTryParseExtended) &&
               SymbolEqualityComparer.Default.Equals(x.IReferenceTypeWithTryParseExtended,
                   y.IReferenceTypeWithTryParseExtended);
    }

    public int GetHashCode(WellKnownTypes obj)
    {
        return CombineHash(
            obj.FromRouteAttribute,
            obj.FromBodyAttribute,
            obj.FromQueryAttribute,
            obj.IValueTypeWithTryParseExtended,
            obj.IReferenceTypeWithTryParseExtended
        );
    }

    private static int CombineHash(params object?[] objects)
    {
        unchecked
        {
            return objects.Aggregate(17, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
        }
    }
}