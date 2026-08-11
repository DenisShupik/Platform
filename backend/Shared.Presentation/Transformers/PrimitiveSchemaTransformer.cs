using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class PrimitiveSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var jsonType = context.JsonTypeInfo.Type;
        var type = Nullable.GetUnderlyingType(jsonType) ?? jsonType;
        var schemaType = JsonSchemaType.Integer;
        if (type != jsonType) schemaType |= JsonSchemaType.Null;

        if (type == typeof(long))
        {
            schema.Type = schemaType;
            schema.Format = "int64";
            schema.Pattern = null;
        }
        else if (type == typeof(ulong))
        {
            schema.Type = schemaType;
            schema.Format = "uint64";
            schema.Pattern = null;
        }
        else if (type == typeof(int))
        {
            schema.Type = schemaType;
            schema.Format = "int32";
            schema.Pattern = null;
        }
        else if (type == typeof(uint))
        {
            schema.Type = schemaType;
            schema.Format = "uint32";
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}
