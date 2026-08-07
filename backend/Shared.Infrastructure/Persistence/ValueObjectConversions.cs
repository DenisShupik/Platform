using System.Reflection;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Persistence;

public static class ValueObjectConversions
{
    public static ModelConfigurationBuilder ConfigureValueObjects(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        configurationBuilder.ConfigureVogenValueObjects(assemblies);
        configurationBuilder.ConfigureEnumSets(assemblies);

        return configurationBuilder;
    }

    public static MappingSchema CreateLinqToDbMappingSchema(params Assembly[] assemblies)
    {
        var mappingSchema = VogenValueObjectConversions.CreateLinqToDbMappingSchema(assemblies);
        mappingSchema.ConfigureEnumSets(assemblies);

        return mappingSchema;
    }
}
