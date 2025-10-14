using System.Reflection;
using System.Text.Json.Nodes;
using JasperFx.Core;
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
            Transform(nullableTypeSchema, underlyingType);
            var nullableTypeSchemaId = nullableTypeSchema.GetOpenApiSchemaId();
            context.Document?.Components?.Schemas?.TryAdd(nullableTypeSchemaId, nullableTypeSchema);
            context.Document?.Workspace?.RegisterComponentForDocument(context.Document, nullableTypeSchema, nullableTypeSchemaId);
            schema.Type = nullableTypeSchema.Type;
            schema.Enum = nullableTypeSchema.Enum;
            schema.Extensions = nullableTypeSchema.Extensions;
        }

        if (!type.IsEnum) return;

        Transform(schema, type);
        if (context.Document == null) throw new OpenApiException("Document cannot be null");
        var schemaId = schema.GetOpenApiSchemaId();
        context.Document.Components?.Schemas?.TryAdd(schemaId, schema);
        context.Document.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
    }

    private static void Transform(OpenApiSchema schema, Type type)
    {
        var names = Enum.GetNames(type);
        schema.Type = JsonSchemaType.String;
        schema.Extensions = new Dictionary<string, IOpenApiExtension>();
        schema.Enum = new List<JsonNode>();
        foreach (var value in names)
        {
            schema.Enum.Add(value.ToLowerInvariant());
        }

        var varNames = new JsonArray();
        varNames.AddRange(names.Select(name => JsonValue.Create(name.ToUpperInvariant())));
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(varNames);

        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata["x-schema-id"] = type.Name;
    }
}