using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class DictionarySchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Document == null) return;
        var type = context.JsonTypeInfo.Type;

        if (!type.IsGenericType) return;

        var typeDefinition = type.GetGenericTypeDefinition();

        if (typeDefinition != typeof(Dictionary<,>)) return;
        var keyType = type.GetGenericArguments()[0];
        var valueType = type.GetGenericArguments()[1];

        var keySchema = await context.GetOrCreateSchemaAsync(keyType, null, cancellationToken);
        var valueSchema = await context.GetOrCreateSchemaAsync(valueType, null, cancellationToken);
        
        schema.Type = JsonSchemaType.Object;
        
        var keySchemaId = keySchema.GetOpenApiSchemaId();
        
        schema.Extensions = new Dictionary<string, IOpenApiExtension>();
        if (!string.IsNullOrEmpty(keySchemaId))
        {
            var refObj = new JsonObject
            {
                ["$ref"] = new OpenApiSchemaReference(keySchemaId,context.Document).Reference.ReferenceV3
            };
           
            schema.Extensions["propertyNames"] = new JsonNodeExtension(refObj);
        }
        
        schema.AdditionalProperties = valueSchema;
    }
}