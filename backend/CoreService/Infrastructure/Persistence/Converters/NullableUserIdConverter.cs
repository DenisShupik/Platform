using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Converters;

public sealed class NullableUserIdConverter : ValueConverter<UserId?, Guid?>
{
    public NullableUserIdConverter()
        : base(
            v => v == null ? null : v.Value.Value,
            v => v == null ? null : UserId.From(v.Value))
    {
    }
}