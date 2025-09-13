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
        return schemaIdValue as string ?? throw new ArgumentException("x-schema-id is not string");
    }

    public static string? TryGetOpenApiSchemaId(this OpenApiSchema schema)
    {
        if (schema.Metadata != null && schema.Metadata.TryGetValue(SchemaIdKey, out var schemaIdValue) &&
            schemaIdValue is string schemaId) return schemaId;
        return null;
    }

    public static OpenApiSchemaReference CreateOpenApiReference(this OpenApiDocument document, OpenApiSchema schema)
    {
        var schemaId = GetOpenApiSchemaId(schema);
        return new OpenApiSchemaReference(schemaId, document);
    }


    public static void RegisterOpenApiSchema(this OpenApiDocument document, OpenApiSchema schema)
    {
        var schemaId = schema.TryGetOpenApiSchemaId();
        if (schemaId == null) throw new KeyNotFoundException(KeyNotFoundErrorMessage);
        document.Workspace?.RegisterComponentForDocument(document, schema, schemaId);
    }
}