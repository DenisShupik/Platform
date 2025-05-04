using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddRequiredSchemaFilter : ISchemaFilter
{
    private readonly NullabilityInfoContext _nullabilityInfoContext = new();

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
            return;

        schema.Required ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var propertyInfos = context.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var (propertyName, propertySchema) in schema.Properties)
        {
            if (!propertySchema.Nullable
                || (propertyInfos.TryGetValue(propertyName, out var propertyInfo)
                    && (propertyInfo.PropertyType.IsValueType
                        && Nullable.GetUnderlyingType(propertyInfo.PropertyType) == null
                        || _nullabilityInfoContext.Create(propertyInfo).WriteState != NullabilityState.Nullable))
               )
            {
                propertySchema.Nullable = false;
                schema.Required.Add(propertyName);
            }
        }
    }
}