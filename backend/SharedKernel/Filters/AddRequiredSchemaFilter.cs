using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Filters;

public sealed class AddRequiredSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
            return;

        foreach (var schemaProp in schema.Properties)
        {
            if (schemaProp.Value.Nullable)
                continue;

            schema.Required.Add(schemaProp.Key);
        }
    }
}