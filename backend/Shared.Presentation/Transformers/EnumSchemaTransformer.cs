using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            if (!underlyingType.IsEnum) return;
            var nullableTypeSchema = await context.GetOrCreateSchemaAsync(underlyingType, null, cancellationToken);
            schema.Type = null;
            schema.Format = null;
            schema.Properties = null;
            schema.Required = null;
            schema.Enum = null;
            schema.AllOf = null;
            schema.AnyOf = null;
            schema.OneOf =
            [
                nullableTypeSchema,
                new OpenApiSchema { Type = JsonSchemaType.Null }
            ];
            return;
        }

        if (!type.IsEnum) return;

        Transform(schema, type);
    }

    private static void Transform(OpenApiSchema schema, Type type)
    {
        var names = Enum.GetNames(type);
        schema.Type = JsonSchemaType.String;
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Enum = new List<JsonNode>();
        foreach (var value in names)
        {
            schema.Enum.Add(value.ToCamelCase());
        }

        var varNames = new JsonArray();
        varNames.AddRange(names.Select(name => JsonValue.Create(name.ToUpperSnakeCase())));
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(varNames);

    }
}
