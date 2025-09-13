using System.Text.Json.Nodes;
using JasperFx.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class EnumTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        var underlyingType = Nullable.GetUnderlyingType(type);
        Type? nullableType = null;
        if (underlyingType != null)
        {
            nullableType = type;
            type = underlyingType;
        }

        if (!type.IsEnum) return;

        if (nullableType != null)
        {
            if (!(context.Document?.Components?.Schemas?.TryGetValue(type.Name, out _) ?? false))
            {
                var newSchema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);
                Transform(newSchema, type);
                if (context.Document == null) throw new NullReferenceException();
                var newSchemaId = newSchema.GetOpenApiSchemaId();
                context.Document.Components?.Schemas?.TryAdd(newSchemaId, newSchema);
                context.Document.Workspace?.RegisterComponentForDocument(context.Document, newSchema, newSchemaId);
            }

            schema.Type = null;
            schema.Properties = null;
            schema.Required = null;
            schema.AnyOf =
                new List<IOpenApiSchema>
                {
                    new OpenApiSchemaReference(type.Name, context.Document),
                    new OpenApiSchema { Type = JsonSchemaType.Null }
                };

            schema.Metadata?.Clear();
        }
        else
        {
            Transform(schema, type);
            if (context.Document == null) throw new NullReferenceException();
            var schemaId = schema.GetOpenApiSchemaId();
            context.Document.Components?.Schemas?.TryAdd(schemaId, schema);
            context.Document.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
        }
    }

    private static void Transform(OpenApiSchema schema, Type type)
    {
        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type);
        schema.Type = JsonSchemaType.Integer;
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