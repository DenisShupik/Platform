using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreService.Infrastructure.Persistence.Converters;

public sealed class NullableThreadPolicySetIdConverter : ValueConverter<ThreadPolicySetId?, Guid?>
{
    public NullableThreadPolicySetIdConverter()
        : base(
            v => v == null ? null : v.Value.Value,
            v => v == null ? null : ThreadPolicySetId.From(v.Value))
    {
    }
}