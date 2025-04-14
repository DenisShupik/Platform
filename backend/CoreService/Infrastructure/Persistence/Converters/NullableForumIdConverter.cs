using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreService.Infrastructure.Persistence.Converters;

public sealed class NullableForumIdConverter : ValueConverter<ForumId?, Guid?>
{
    public NullableForumIdConverter()
        : base(
            v => v == null ? null : v.Value.Value,
            v => v == null ? null : ForumId.From(v.Value))
    {
    }
}