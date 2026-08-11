using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class CollectionSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Items is not null) return;

        var itemType = GetItemType(context.JsonTypeInfo.Type);
        if (itemType is null) return;

        var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
        schema.Type = JsonSchemaType.Array;
        schema.Items = itemSchema;
    }

    private static Type? GetItemType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        if (!type.IsGenericType) return null;

        var typeDefinition = type.GetGenericTypeDefinition();
        return typeDefinition == typeof(IEnumerable<>) ||
               typeDefinition == typeof(IReadOnlyCollection<>) ||
               typeDefinition == typeof(IReadOnlyList<>) ||
               typeDefinition == typeof(ICollection<>) ||
               typeDefinition == typeof(IList<>) ||
               typeDefinition == typeof(List<>)
            ? type.GetGenericArguments()[0]
            : null;
    }
}
