using System.Text.Json.Nodes;
using JasperFx.Core;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SharedKernel.Application.Abstractions;

namespace SharedKernel.Presentation.Transformers;

public sealed class SortCriteriaTransformer : IOpenApiSchemaTransformer
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
        
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SortCriteria<>)) return;
        var primitive = type.GetGenericArguments()[0];
        var names = Enum.GetNames(primitive);
        
        if (nullableType != null)
        {
            if (!(context.Document?.Components?.Schemas?.TryGetValue(primitive.Name, out _) ?? false))
            {
                var newSchema = await context.GetOrCreateSchemaAsync(primitive, null, cancellationToken);
                Transform(newSchema, names, primitive);
                context.Document?.Components?.Schemas?.Add(primitive.Name, newSchema);
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
        schema.Metadata["x-schema-id"] = type.Name;
    }
}