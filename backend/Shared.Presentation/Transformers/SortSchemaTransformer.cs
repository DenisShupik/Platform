using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Abstractions;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class SortSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var declaredType = context.JsonTypeInfo.Type;
        var type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        if (!type.IsGenericType) return;

        var typeDefinition = type.GetGenericTypeDefinition();
        if (typeDefinition == typeof(SortCriteria<>))
        {
            var enumType = type.GetGenericArguments()[0];
            if (!enumType.IsEnum)
                throw new OpenApiException($"Sort field type {enumType.FullName} must be an enum");

            if (type != declaredType)
            {
                var document = context.Document ?? throw new OpenApiException("Document cannot be null");
                var sortSchema = CreateSortCriteriaSchema(enumType);
                schema.Type = null;
                schema.Format = null;
                schema.Properties = null;
                schema.Required = null;
                schema.AllOf = null;
                schema.AnyOf = null;
                schema.OneOf =
                [
                    document.GetOrAddSchemaReference(sortSchema),
                    new OpenApiSchema { Type = JsonSchemaType.Null }
                ];
                schema.Metadata?.Clear();
                return;
            }

            ApplySortCriteriaSchema(schema, enumType);
            if (context.Document == null) throw new OpenApiException("Document cannot be null");
            context.Document.GetOrAddSchemaReference(schema);
            return;
        }

        if (typeDefinition != typeof(SortCriteriaList<>)) return;

        var itemType = typeof(SortCriteria<>).MakeGenericType(type.GetGenericArguments()[0]);
        var itemSchema = await context.GetOrCreateSchemaAsync(itemType, null, cancellationToken);
        var itemReference = (context.Document ?? throw new OpenApiException("Document cannot be null"))
            .GetOrAddSchemaReference(itemSchema);

        schema.Type = JsonSchemaType.Array;
        schema.Format = null;
        schema.Properties = null;
        schema.Required = null;
        schema.AllOf = null;
        schema.AnyOf = null;
        schema.OneOf = null;
        schema.Items = itemReference;
        schema.MinItems = 1;
        schema.UniqueItems = true;
    }

    private static OpenApiSchema CreateSortCriteriaSchema(Type enumType)
    {
        var schema = new OpenApiSchema();
        ApplySortCriteriaSchema(schema, enumType);
        return schema;
    }

    private static void ApplySortCriteriaSchema(OpenApiSchema schema, Type enumType)
    {
        var names = Enum.GetNames(enumType);

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Example = null;
        schema.Properties = null;
        schema.Required = null;
        schema.AllOf = null;
        schema.AnyOf = null;
        schema.OneOf = null;
        schema.Items = null;
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();

        schema.Enum = names
            .Select(name => JsonValue.Create(name.ToCamelCase()))
            .Concat(names.Select(name => JsonValue.Create("-" + name.ToCamelCase())))
            .ToArray<JsonNode>();

        var variableNames = new JsonArray();
        variableNames.AddRange(names.Select(name => JsonValue.Create($"{name.ToUpperSnakeCase()}_ASC")));
        variableNames.AddRange(names.Select(name => JsonValue.Create($"{name.ToUpperSnakeCase()}_DESC")));
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(variableNames);

        var descriptions = new JsonArray();
        descriptions.AddRange(names.Select(name => JsonValue.Create($"Sort by {name} ascending")));
        descriptions.AddRange(names.Select(name => JsonValue.Create($"Sort by {name} descending")));
        schema.Extensions["x-enum-descriptions"] = new JsonNodeExtension(descriptions);

        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata["x-schema-id"] = enumType.DeclaringType is null
            ? enumType.Name
            : enumType.DeclaringType.Name + enumType.Name;
    }
}
