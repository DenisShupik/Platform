using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class SchemaReferenceIdTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var referenceId = OpenApiSchemaReferenceId.Create(context.JsonTypeInfo);
        if (referenceId is not null)
            OpenApiSchemaReferenceId.Set(schema, referenceId);

        return Task.CompletedTask;
    }
}
