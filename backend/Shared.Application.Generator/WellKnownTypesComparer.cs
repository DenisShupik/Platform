using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Shared.Application.Generator;

internal sealed class WellKnownTypesComparer : IEqualityComparer<WellKnownTypes>
{
    public static readonly WellKnownTypesComparer Instance = new();

    public bool Equals(WellKnownTypes? x, WellKnownTypes? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return
            SymbolEqualityComparer.Default.Equals(x.Query, y.Query) &&
            SymbolEqualityComparer.Default.Equals(x.Command, y.Command) &&
            SymbolEqualityComparer.Default.Equals(x.QueryHandler, y.QueryHandler) &&
            SymbolEqualityComparer.Default.Equals(x.CommandHandler, y.CommandHandler)
            ;
    }

    public int GetHashCode(WellKnownTypes obj)
    {
        return CombineHash(
            obj.Query,
            obj.Command,
            obj.QueryHandler,
            obj.CommandHandler
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