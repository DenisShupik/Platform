using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreService.Infrastructure.Persistence.Converters;

public sealed class NullableCategoryPolicySetIdConverter : ValueConverter<CategoryPolicySetId?, Guid?>
{
    public NullableCategoryPolicySetIdConverter()
        : base(
            v => v == null ? null : v.Value.Value,
            v => v == null ? null : CategoryPolicySetId.From(v.Value))
    {
    }
}