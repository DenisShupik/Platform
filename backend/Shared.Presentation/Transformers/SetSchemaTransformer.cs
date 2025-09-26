using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class SetSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Document == null) return;
        var type = context.JsonTypeInfo.Type;

        if (!type.IsGenericType) return;

        var typeDefinition = type.GetGenericTypeDefinition();

        if (typeDefinition != typeof(IdSet<,>) && typeDefinition != typeof(EnumSet<>)) return;
        var itemType = type.GetGenericArguments()[0];

        var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
        var itemSchemaId = itemSchema.TryGetOpenApiSchemaId();
        if (itemSchemaId is not null)
        {
            context.Document.Components?.Schemas?.TryAdd(itemSchemaId, schema);
            context.Document.Workspace?.RegisterComponentForDocument(context.Document, itemSchema, itemSchemaId);
        }
        
        schema.Type = JsonSchemaType.Array;
        schema.Items = context.Document.CreateOpenApiReference(itemSchema);
        schema.MinItems = 1;
        schema.UniqueItems = true;
        schema.Metadata?.Clear();
    }
}