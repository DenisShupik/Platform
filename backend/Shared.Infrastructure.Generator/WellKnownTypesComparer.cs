using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Shared.Infrastructure.Generator;

internal sealed class WellKnownTypesComparer : IEqualityComparer<WellKnownTypes>
{
    public static readonly WellKnownTypesComparer Instance = new();

    public bool Equals(WellKnownTypes? x, WellKnownTypes? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return SymbolEqualityComparer.Default.Equals(x.SingleSortPagedQuery, y.SingleSortPagedQuery) &&
               SymbolEqualityComparer.Default.Equals(x.MultiSortPagedQuery, y.MultiSortPagedQuery) &&
               SymbolEqualityComparer.Default.Equals(x.SortExpressionAttribute, y.SortExpressionAttribute)
            ;
    }

    public int GetHashCode(WellKnownTypes obj)
    {
        return CombineHash(
            obj.SingleSortPagedQuery,
            obj.MultiSortPagedQuery,
            obj.SortExpressionAttribute
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