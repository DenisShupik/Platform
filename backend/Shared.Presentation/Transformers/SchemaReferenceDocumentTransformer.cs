using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

/// <summary>
/// Promotes reusable schemas introduced by operation transformers to components.
/// ASP.NET Core performs this step for ApiExplorer schemas, but not for schemas
/// created later by custom <see cref="IOpenApiOperationTransformer"/> instances.
/// </summary>
public sealed class SchemaReferenceDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var pending = new Queue<(string Id, OpenApiSchema Schema)>(
            (document.Components?.Schemas ?? new Dictionary<string, IOpenApiSchema>())
            .Where(static component => component.Value is OpenApiSchema)
            .Select(static component => (component.Key, (OpenApiSchema)component.Value)));
        var visited = new HashSet<OpenApiSchema>(ReferenceEqualityComparer.Instance);

        NormalizePendingComponents(document, pending, visited);
        NormalizePathItems(document, document.Paths?.Values, pending, visited);
        NormalizePathItems(document, document.Webhooks?.Values, pending, visited);

        if (document.Components is { } components)
        {
            NormalizeParameters(document, components.Parameters?.Values, pending, visited);
            NormalizeRequestBodies(document, components.RequestBodies?.Values, pending, visited);
            NormalizeResponses(document, components.Responses?.Values, pending, visited);
            NormalizeHeaders(document, components.Headers?.Values, pending, visited);
            NormalizeMediaTypes(document, components.MediaTypes?.Values, pending, visited);
            NormalizeCallbacks(document, components.Callbacks?.Values, pending, visited);
            NormalizePathItems(document, components.PathItems?.Values, pending, visited);
        }

        NormalizePendingComponents(document, pending, visited);
        return Task.CompletedTask;
    }

    private static void NormalizePendingComponents(
        OpenApiDocument document,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        while (pending.TryDequeue(out var component))
            NormalizeNestedSchemas(document, component.Schema, component.Id, pending, visited);
    }

    private static void NormalizePathItems(
        OpenApiDocument document,
        IEnumerable<IOpenApiPathItem>? pathItems,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (pathItems is null) return;

        foreach (var pathItem in pathItems.OfType<OpenApiPathItem>())
        {
            NormalizeParameters(document, pathItem.Parameters, pending, visited);
            if (pathItem.Operations is null) continue;

            foreach (var operation in pathItem.Operations.Values)
            {
                NormalizeParameters(document, operation.Parameters, pending, visited);
                NormalizeRequestBodies(document, [operation.RequestBody], pending, visited);
                NormalizeResponses(document, operation.Responses?.Values, pending, visited);
                NormalizeCallbacks(document, operation.Callbacks?.Values, pending, visited);
            }
        }
    }

    private static void NormalizeCallbacks(
        OpenApiDocument document,
        IEnumerable<IOpenApiCallback>? callbacks,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (callbacks is null) return;

        foreach (var callback in callbacks.OfType<OpenApiCallback>())
            NormalizePathItems(document, callback.PathItems?.Values, pending, visited);
    }

    private static void NormalizeParameters(
        OpenApiDocument document,
        IEnumerable<IOpenApiParameter>? parameters,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (parameters is null) return;

        foreach (var parameter in parameters.OfType<OpenApiParameter>())
        {
            if (parameter.Schema is not null)
                parameter.Schema = NormalizeSchema(document, parameter.Schema, null, pending, visited);
            NormalizeContent(document, parameter.Content, pending, visited);
        }
    }

    private static void NormalizeRequestBodies(
        OpenApiDocument document,
        IEnumerable<IOpenApiRequestBody?>? requestBodies,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (requestBodies is null) return;

        foreach (var requestBody in requestBodies.OfType<OpenApiRequestBody>())
            NormalizeContent(document, requestBody.Content, pending, visited);
    }

    private static void NormalizeResponses(
        OpenApiDocument document,
        IEnumerable<IOpenApiResponse>? responses,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (responses is null) return;

        foreach (var response in responses.OfType<OpenApiResponse>())
        {
            NormalizeContent(document, response.Content, pending, visited);
            NormalizeHeaders(document, response.Headers?.Values, pending, visited);
        }
    }

    private static void NormalizeHeaders(
        OpenApiDocument document,
        IEnumerable<IOpenApiHeader>? headers,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (headers is null) return;

        foreach (var header in headers.OfType<OpenApiHeader>())
        {
            if (header.Schema is not null)
                header.Schema = NormalizeSchema(document, header.Schema, null, pending, visited);
            NormalizeContent(document, header.Content, pending, visited);
        }
    }

    private static void NormalizeContent(
        OpenApiDocument document,
        IDictionary<string, IOpenApiMediaType>? content,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (content is not null)
            NormalizeMediaTypes(document, content.Values, pending, visited);
    }

    private static void NormalizeMediaTypes(
        OpenApiDocument document,
        IEnumerable<IOpenApiMediaType>? mediaTypes,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (mediaTypes is null) return;

        foreach (var mediaType in mediaTypes.OfType<OpenApiMediaType>())
        {
            if (mediaType.Schema is not null)
                mediaType.Schema = NormalizeSchema(document, mediaType.Schema, null, pending, visited);
            if (mediaType.ItemSchema is not null)
                mediaType.ItemSchema = NormalizeSchema(document, mediaType.ItemSchema, null, pending, visited);

            NormalizeEncodings(document, mediaType.Encoding?.Values, pending, visited);
            NormalizeEncodings(document, [mediaType.ItemEncoding], pending, visited);
            NormalizeEncodings(document, mediaType.PrefixEncoding, pending, visited);
        }
    }

    private static void NormalizeEncodings(
        OpenApiDocument document,
        IEnumerable<OpenApiEncoding?>? encodings,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (encodings is null) return;

        foreach (var encoding in encodings.OfType<OpenApiEncoding>())
        {
            NormalizeHeaders(document, encoding.Headers?.Values, pending, visited);
            NormalizeEncodings(document, encoding.Encoding?.Values, pending, visited);
            NormalizeEncodings(document, [encoding.ItemEncoding], pending, visited);
            NormalizeEncodings(document, encoding.PrefixEncoding, pending, visited);
        }
    }

    private static void NormalizeNestedSchemas(
        OpenApiDocument document,
        OpenApiSchema schema,
        string? owningSchemaId,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (!visited.Add(schema)) return;

        NormalizeSchemaDictionary(document, schema.Properties, owningSchemaId, pending, visited);
        NormalizeSchemaDictionary(document, schema.PatternProperties, owningSchemaId, pending, visited);
        NormalizeSchemaDictionary(document, schema.Definitions, owningSchemaId, pending, visited);
        NormalizeSchemaDictionary(document, schema.DependentSchemas, owningSchemaId, pending, visited);

        NormalizeSchemaList(document, schema.AllOf, owningSchemaId, pending, visited);
        NormalizeSchemaList(document, schema.AnyOf, owningSchemaId, pending, visited);
        NormalizeSchemaList(document, schema.OneOf, owningSchemaId, pending, visited);

        schema.Items = NormalizeNullableSchema(document, schema.Items, owningSchemaId, pending, visited);
        schema.AdditionalProperties = NormalizeNullableSchema(
            document, schema.AdditionalProperties, owningSchemaId, pending, visited);
        schema.PropertyNames = NormalizeNullableSchema(document, schema.PropertyNames, owningSchemaId, pending, visited);
        schema.Contains = NormalizeNullableSchema(document, schema.Contains, owningSchemaId, pending, visited);
        schema.ContentSchema = NormalizeNullableSchema(document, schema.ContentSchema, owningSchemaId, pending, visited);
        schema.If = NormalizeNullableSchema(document, schema.If, owningSchemaId, pending, visited);
        schema.Then = NormalizeNullableSchema(document, schema.Then, owningSchemaId, pending, visited);
        schema.Else = NormalizeNullableSchema(document, schema.Else, owningSchemaId, pending, visited);
        schema.Not = NormalizeNullableSchema(document, schema.Not, owningSchemaId, pending, visited);
        schema.UnevaluatedPropertiesSchema = NormalizeNullableSchema(
            document, schema.UnevaluatedPropertiesSchema, owningSchemaId, pending, visited);
    }

    private static void NormalizeSchemaDictionary(
        OpenApiDocument document,
        IDictionary<string, IOpenApiSchema>? schemas,
        string? owningSchemaId,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (schemas is null) return;

        foreach (var entry in schemas.ToArray())
            schemas[entry.Key] = NormalizeSchema(document, entry.Value, owningSchemaId, pending, visited);
    }

    private static void NormalizeSchemaList(
        OpenApiDocument document,
        IList<IOpenApiSchema>? schemas,
        string? owningSchemaId,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (schemas is null) return;

        for (var index = 0; index < schemas.Count; index++)
            schemas[index] = NormalizeSchema(document, schemas[index], owningSchemaId, pending, visited);
    }

    private static IOpenApiSchema? NormalizeNullableSchema(
        OpenApiDocument document,
        IOpenApiSchema? schema,
        string? owningSchemaId,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited) =>
        schema is null ? null : NormalizeSchema(document, schema, owningSchemaId, pending, visited);

    private static IOpenApiSchema NormalizeSchema(
        OpenApiDocument document,
        IOpenApiSchema schema,
        string? owningSchemaId,
        Queue<(string Id, OpenApiSchema Schema)> pending,
        HashSet<OpenApiSchema> visited)
    {
        if (schema is not OpenApiSchema concreteSchema) return schema;

        var schemaId = OpenApiSchemaReferenceId.Get(concreteSchema);
        if (schemaId is null || schemaId == owningSchemaId)
        {
            NormalizeNestedSchemas(document, concreteSchema, owningSchemaId, pending, visited);
            return concreteSchema;
        }

        if (document.Components?.Schemas?.ContainsKey(schemaId) != true)
        {
            if (!document.AddComponent(schemaId, concreteSchema))
                throw new OpenApiException($"Schema component '{schemaId}' could not be registered");
            pending.Enqueue((schemaId, concreteSchema));
        }

        return new OpenApiSchemaReference(schemaId, document);
    }
}
