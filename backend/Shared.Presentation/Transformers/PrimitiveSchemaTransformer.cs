using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class PrimitiveSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type == typeof(long))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "int64";
            schema.Pattern = null;
        }
        else if (type == typeof(ulong))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "uint64";
            schema.Pattern = null;
        }
        else if (type == typeof(int))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "int32";
            schema.Pattern = null;
        }
        else if (type == typeof(uint))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "uint32";
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}