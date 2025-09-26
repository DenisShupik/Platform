using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Abstractions;

namespace NotificationService.Infrastructure.Persistence.Converters;

public sealed class EnumSetConverter<T> : ValueConverter<EnumSet<T>, HashSet<T>>
    where T : struct, Enum
{
    public EnumSetConverter()
        : base(
            v => v,
            v => new(v)
        )
    {
    }
}

public sealed class EnumSetValueComparer<T> : ValueComparer<EnumSet<T>>
    where T : struct, Enum
{
    public EnumSetValueComparer()
        : base(
            (v1, v2) => ReferenceEquals(v1, v2) || (v1 != null && v2 != null && v1.SetEquals(v2)),
            v => v.Count == 0
                ? 0
                : v.OrderBy(x => Convert.ToInt64(x)).Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
            v => new(v)
        )
    {
    }
}