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

        if (typeDefinition != typeof(Result<,,,>)) return;
        var valueType = type.GetGenericArguments()[0];

        var valueSchema = await context.GetOrCreateSchemaAsync(valueType, null, cancellationToken);
        var valueSchemaId = valueSchema.TryGetOpenApiSchemaId();
        if (valueSchemaId is not null)
        {
            context.Document.Components?.Schemas?.TryAdd(valueSchemaId, valueSchema);
            context.Document.Workspace?.RegisterComponentForDocument(context.Document, valueSchema, valueSchemaId);
        }

        schema.Type = JsonSchemaType.Object;
        schema.Properties = new Dictionary<string, IOpenApiSchema>();
        schema.Properties.Add("value", valueSchema);
        schema.Metadata?.Clear();
    }
}