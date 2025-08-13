using Microsoft.OpenApi.Models;
using SharedKernel.Application.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddIdSetSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IdSet<>)) return;

        var itemType = type.GetGenericArguments()[0];

        var itemSchema = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);

        schema.Type = "array";
        schema.Items = itemSchema;
        schema.MinItems = 1;
        schema.UniqueItems = true;
        schema.Nullable = false;
    }
}