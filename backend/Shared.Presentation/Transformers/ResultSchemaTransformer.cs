using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions.Results;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class ResultSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Document == null) return;
        var type = context.JsonTypeInfo.Type;

        if (!type.IsGenericType) return;
        
        var typeDefinition = type.GetGenericTypeDefinition();
        
        if (!typeof(IResult).IsAssignableFrom(typeDefinition)) return;
        
        var valueType = type.GetGenericArguments()[0];
        var errorTypes = type.GetGenericArguments().Skip(1);

        var valueSchema = await context.GetOrCreateSchemaAsync(valueType, null, cancellationToken);
        var valueSchemaId = valueSchema.TryGetOpenApiSchemaId();
        if (valueSchemaId is not null)
        {
            context.Document?.Components?.Schemas?.TryAdd(valueSchemaId, valueSchema);
            context.Document?.Workspace?.RegisterComponentForDocument(context.Document, valueSchema, valueSchemaId);
        }

        schema.Type = JsonSchemaType.Object;
        schema.Properties = new Dictionary<string, IOpenApiSchema>();
        schema.Properties.Add("value", valueSchema);

        var list = new List<IOpenApiSchema>();
        await GetErrorSchemaAsync(errorTypes, context, list, cancellationToken);

        var errorsSchema = new OpenApiSchema
        {
            OneOf = list,
            Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
        };
        schema.Properties.Add("error", errorsSchema);
        schema.Metadata?.Clear();
    }

    private static async Task GetErrorSchemaAsync(IEnumerable<Type> errorTypes, OpenApiSchemaTransformerContext context,
        List<IOpenApiSchema> list, CancellationToken cancellationToken)
    {
        foreach (var errorType in errorTypes)
        {
            var schema = await context.GetOrCreateSchemaAsync(errorType, null, cancellationToken);

            var schemaId = schema.GetOpenApiSchemaId();

            if (schema.Properties != null)
                foreach (var key in schema.Properties.Keys)
                {
                    schema.Properties.TryGetValue(key, out var value);
                    if (value is not OpenApiSchema propSchema) continue;
                    var propSchemaId = propSchema.TryGetOpenApiSchemaId();
                    if (string.IsNullOrEmpty(propSchemaId)) continue;
                    context.Document?.Components?.Schemas?.TryAdd(propSchemaId, propSchema);
                    context.Document?.Workspace?.RegisterComponentForDocument(context.Document, propSchema,
                        propSchemaId);
                    var refPropSchema = new OpenApiSchemaReference(propSchemaId, context.Document);
                    schema.Properties[key] = refPropSchema;
                }

            if (schema.AnyOf != null)
                for (var i = 0; i < schema.AnyOf.Count; i++)
                {
                    if (schema.AnyOf[i] is not OpenApiSchema propSchema) continue;
                    var propSchemaId = propSchema.TryGetOpenApiSchemaId();
                    if (string.IsNullOrEmpty(propSchemaId)) continue;
                    context.Document?.Components?.Schemas?.TryAdd(propSchemaId, propSchema);
                    context.Document?.Workspace?.RegisterComponentForDocument(context.Document, propSchema,
                        propSchemaId);
                    var refPropSchema = new OpenApiSchemaReference(propSchemaId, context.Document);
                    schema.AnyOf[i] = refPropSchema;
                }

            context.Document?.Components?.Schemas?.TryAdd(schemaId, schema);
            context.Document?.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
            list.Add(new OpenApiSchemaReference(schemaId, context.Document));
        }
    }
}