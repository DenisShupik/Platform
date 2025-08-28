using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SharedKernel.Application.Abstractions;

namespace SharedKernel.Presentation.Transformers;

public sealed class SetTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsGenericType) return;

        var typeDefinition = type.GetGenericTypeDefinition();

        if (typeDefinition != typeof(IdSet<>) && typeDefinition != typeof(EnumSet<>)) return;
        var itemType = type.GetGenericArguments()[0];

        var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);

        schema.Type = JsonSchemaType.Array;
        schema.Items = itemSchema;
        schema.MinItems = 1;
        schema.UniqueItems = true;
    }
}