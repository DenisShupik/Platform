using System.Text.Json.Nodes;
using JasperFx.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum) return Task.CompletedTask;

        Transform(schema, type);
        if (context.Document == null) throw new OpenApiException("Document cannot be null");
        var schemaId = schema.GetOpenApiSchemaId();
        context.Document.Components?.Schemas?.TryAdd(schemaId, schema);
        context.Document.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
        return Task.CompletedTask;
    }

    private static void Transform(OpenApiSchema schema, Type type)
    {
        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type);
        schema.Type = JsonSchemaType.Integer;
        schema.Format = "int32";
        schema.Extensions = new Dictionary<string, IOpenApiExtension>();
        schema.Enum = new List<JsonNode>();
        foreach (var value in values)
        {
            schema.Enum.Add(Convert.ToInt32(value));
        }

        var varNames = new JsonArray();
        varNames.AddRange(names.Select(name => JsonValue.Create(name)));
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(varNames);

        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata["x-schema-id"] = type.Name;
    }
}