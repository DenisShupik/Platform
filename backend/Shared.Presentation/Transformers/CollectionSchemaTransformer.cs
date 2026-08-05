using System.Collections.Generic;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class CollectionSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.Document == null || schema.Items != null) return;

        var itemType = GetItemType(context.JsonTypeInfo.Type);
        if (itemType == null) return;

        var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
        var itemSchemaId = itemSchema.TryGetOpenApiSchemaId();
        if (!string.IsNullOrEmpty(itemSchemaId))
        {
            context.Document.Components?.Schemas?.TryAdd(itemSchemaId, itemSchema);
            context.Document.Workspace?.RegisterComponentForDocument(context.Document, itemSchema, itemSchemaId);
        }

        schema.Type = JsonSchemaType.Array;
        schema.Items = context.Document.CreateOpenApiReference(itemSchema);
        schema.Metadata?.Clear();
    }

    private static Type? GetItemType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        if (!type.IsGenericType) return null;

        var typeDefinition = type.GetGenericTypeDefinition();
        if (
            typeDefinition != typeof(IEnumerable<>) &&
            typeDefinition != typeof(IReadOnlyCollection<>) &&
            typeDefinition != typeof(IReadOnlyList<>) &&
            typeDefinition != typeof(ICollection<>) &&
            typeDefinition != typeof(IList<>) &&
            typeDefinition != typeof(List<>)
        )
            return null;

        return type.GetGenericArguments()[0];
    }
}
