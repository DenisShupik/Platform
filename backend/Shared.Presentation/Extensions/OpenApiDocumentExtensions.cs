using Microsoft.OpenApi;

namespace Shared.Presentation.Extensions;

public static class OpenApiDocumentExtensions
{
    private const string SchemaIdKey = "x-schema-id";

    private const string KeyNotFoundErrorMessage = $"{SchemaIdKey} not found";

    public static string GetOpenApiSchemaId(this OpenApiSchema schema)
    {
        if (schema.Metadata == null) throw new NullReferenceException("Metadata not found");
        if (!schema.Metadata.TryGetValue(SchemaIdKey, out var schemaIdValue))
            throw new KeyNotFoundException(KeyNotFoundErrorMessage);
        if (schemaIdValue is not string schemaId)
            throw new ArgumentException($"{SchemaIdKey} is not a string");
        if (string.IsNullOrWhiteSpace(schemaId))
            throw new ArgumentException($"{SchemaIdKey} is empty");
        return schemaId;
    }

    public static string? TryGetOpenApiSchemaId(this OpenApiSchema schema)
    {
        if (schema.Metadata != null && schema.Metadata.TryGetValue(SchemaIdKey, out var schemaIdValue) &&
            schemaIdValue is string schemaId && !string.IsNullOrWhiteSpace(schemaId)) return schemaId;
        return null;
    }

    public static void SetOpenApiSchemaId(this OpenApiSchema schema, string schemaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaId);
        schema.Metadata ??= new Dictionary<string, object>();
        schema.Metadata[SchemaIdKey] = schemaId;
    }

    public static OpenApiSchemaReference GetOrAddSchemaReference(
        this OpenApiDocument document,
        OpenApiSchema schema)
    {
        var schemaId = GetOpenApiSchemaId(schema);

        if (document.Components?.Schemas?.TryGetValue(schemaId, out var existingSchema) == true)
        {
            if (existingSchema is OpenApiSchema concreteExistingSchema)
                schema = concreteExistingSchema;
        }
        else if (!document.AddComponent(schemaId, schema))
        {
            throw new OpenApiException($"Schema component '{schemaId}' could not be registered");
        }

        return new OpenApiSchemaReference(schemaId, document);
    }
}
