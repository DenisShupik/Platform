using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddRequiredSchemaFilter : ISchemaFilter
{
    private readonly NullabilityInfoContext _nullCtx = new();

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null)
            return;

        schema.Required ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
        var propMap = context.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var (name, propSchema) in schema.Properties)
        {
            if (!propSchema.Nullable
                || (propMap.TryGetValue(name, out var pi)
                    && (pi.PropertyType.IsValueType && Nullable.GetUnderlyingType(pi.PropertyType) == null
                        || _nullCtx.Create(pi).WriteState != NullabilityState.Nullable)))
            {
                propSchema.Nullable = false;
                schema.Required.Add(name);
            }
        }
    }
}