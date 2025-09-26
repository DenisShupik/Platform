using System.Text.Json.Nodes;
using JasperFx.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Abstractions;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class SortSchemaTransformer : IOpenApiSchemaTransformer
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

        if (!type.IsGenericType) return;
        var typeDefinition = type.GetGenericTypeDefinition();
        if (typeDefinition == typeof(SortCriteria<>))
        {
            var primitive = type.GetGenericArguments()[0];
            var names = Enum.GetNames(primitive);

            if (nullableType != null)
            {
                if (!(context.Document?.Components?.Schemas?.TryGetValue(primitive.Name, out _) ?? false))
                {
                    var newSchema = await context.GetOrCreateSchemaAsync(primitive, null, cancellationToken);
                    Transform(newSchema, names, primitive);

                    if (context.Document == null) throw new NullReferenceException();

                    var schemaId = schema.GetOpenApiSchemaId();
                    context.Document?.Components?.Schemas?.TryAdd(schemaId, newSchema);
                    context.Document?.Workspace?.RegisterComponentForDocument(context.Document, newSchema, schemaId);
                }

                schema.Type = null;
                schema.Properties = null;
                schema.Required = null;
                schema.AnyOf =
                    new List<IOpenApiSchema>
                    {
                        new OpenApiSchemaReference(primitive.Name, context.Document),
                        new OpenApiSchema { Type = JsonSchemaType.Null }
                    };

                schema.Metadata?.Clear();
            }
            else
            {
                Transform(schema, names, primitive);
                var schemaId = schema.GetOpenApiSchemaId();
                context.Document?.Components?.Schemas?.TryAdd(schemaId, schema);
                context.Document?.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
            }
        }
        else if (typeDefinition == typeof(SortCriteriaList<>))
        {
            var primitive = type.GetGenericArguments()[0];

            if (nullableType != null)
            {
                var itemType = typeof(SortCriteria<>).MakeGenericType(primitive);
                var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
                var arraySchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = context.Document?.CreateOpenApiReference(itemSchema),
                    MinItems = 1,
                    UniqueItems = true
                };

                schema.Type = null;
                schema.Properties = null;
                schema.Required = null;
                schema.AnyOf =
                    new List<IOpenApiSchema>
                    {
                        arraySchema,
                        new OpenApiSchema { Type = JsonSchemaType.Null }
                    };
            }
            else
            {
                var itemType = typeof(SortCriteria<>).MakeGenericType(primitive);
                var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
                schema.Type = JsonSchemaType.Array;
                schema.Items = context.Document?.CreateOpenApiReference(itemSchema);
                schema.MinItems = 1;
                schema.UniqueItems = true;
            }

            //schema.Metadata?.Clear();
        }
    }

    private static void Transform(OpenApiSchema schema, string[] names, Type type)
    {
        schema.Type = JsonSchemaType.String;
        schema.Enum = names
            .Select(name => JsonValue.Create(name.ToLowerInvariant()))
            .Concat(names.Select(name => JsonValue.Create("-" + name.ToLowerInvariant())))
            .ToArray<JsonNode>();
        schema.Example = null;
        schema.Extensions = new Dictionary<string, IOpenApiExtension>();

        schema.Enum = names
            .Select(name => JsonValue.Create(name.ToLowerInvariant()))
            .Concat(names.Select(name => JsonValue.Create("-" + name.ToLowerInvariant())))
            .ToArray<JsonNode>();

        var varNames = new JsonArray();
        varNames.AddRange(names.Select(name => JsonValue.Create($"{name}Asc")));
        varNames.AddRange(names.Select(name => JsonValue.Create($"{name}Desc")));
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(varNames);

        var descriptions = new JsonArray();
        descriptions.AddRange(names.Select(name => JsonValue.Create($"Sort by {name} ascending")));
        descriptions.AddRange(names.Select(name => JsonValue.Create($"Sort by {name} descending")));
        schema.Extensions["x-enum-descriptions"] = new JsonNodeExtension(descriptions);

        schema.Required = null;
        schema.Properties = null;

        schema.Metadata ??= new Dictionary<string, object>();

        schema.Metadata["x-schema-id"] = type.DeclaringType == null ? type.Name : type.DeclaringType.Name + type.Name;
    }
}