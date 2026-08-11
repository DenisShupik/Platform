using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Abstractions;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace Shared.Presentation.Transformers;

internal static class OpenApiSchemaReferenceId
{
    private const string MetadataKey = "Platform.SchemaReferenceId";

    public static string? Create(JsonTypeInfo jsonTypeInfo)
    {
        var type = jsonTypeInfo.Type;
        if (!type.IsGenericType)
            return OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);

        var typeDefinition = type.GetGenericTypeDefinition();
        if (typeDefinition == typeof(SortCriteria<>))
        {
            var fieldType = type.GetGenericArguments()[0];
            return fieldType.DeclaringType is null
                ? fieldType.Name
                : fieldType.DeclaringType.Name + fieldType.Name;
        }

        if (typeDefinition == typeof(SortCriteriaList<>) ||
            typeDefinition == typeof(IdSet<,>) ||
            typeDefinition == typeof(EnumSet<>) ||
            typeof(IResult).IsAssignableFrom(typeDefinition))
            return null;

        return OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
    }

    public static void Set(OpenApiSchema schema, string referenceId)
    {
        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata[MetadataKey] = referenceId;
    }

    public static string? Get(OpenApiSchema schema) =>
        schema.Metadata is not null &&
        schema.Metadata.TryGetValue(MetadataKey, out var value) &&
        value is string { Length: > 0 } referenceId
            ? referenceId
            : null;
}
