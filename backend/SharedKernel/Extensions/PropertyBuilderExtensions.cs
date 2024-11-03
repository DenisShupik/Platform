using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SharedKernel.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Guid> SetDefaultUuidValue(this PropertyBuilder<Guid> propertyBuilder) => propertyBuilder.HasDefaultValueSql("gen_random_uuid()");
}