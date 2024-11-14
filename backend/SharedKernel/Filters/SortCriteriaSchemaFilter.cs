using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Sorting;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Filters;

public sealed class SortCriteriaSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(SortCriteria<>)) return;
        var enumType = context.Type.GenericTypeArguments[0];
        schema.Type = "string";
        schema.Description = $"Field to sort by with optional '-' prefix for descending order for {enumType.Name}.";
        schema.Enum = Enum.GetNames(enumType)
            .Select(IOpenApiAny (name) => new OpenApiString(name))
            .Concat(Enum.GetNames(enumType).Select(IOpenApiAny (name) => new OpenApiString($"-{name}")))
            .ToList();
    }
}