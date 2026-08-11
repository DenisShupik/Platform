using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Errors;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class ApiContractOperationTransformer : IOpenApiOperationTransformer
{
    private static readonly Type[] BadRequestTypes =
    [
        typeof(ApiProblemDetails),
        typeof(ApiValidationProblemDetails)
    ];

    private static readonly Type[] LocaleErrorTypes =
    [
        typeof(LocaleRequiredError),
        typeof(UnsupportedLocaleError)
    ];

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!IsApiOperation(context.Description.RelativePath)) return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderNames.AcceptLanguage,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Requested response locale.",
            Schema = CreateLocaleSchema()
        });

        await AddResponseSchemasAsync(
            operation,
            context,
            "400",
            "Invalid request",
            "application/problem+json",
            BadRequestTypes,
            cancellationToken);
        await AddResponseSchemasAsync(
            operation,
            context,
            "406",
            "A supported Accept-Language header is required",
            "application/json",
            LocaleErrorTypes,
            cancellationToken);
        if (operation.RequestBody is not null)
            await AddResponseSchemasAsync(
                operation,
                context,
                "413",
                "Request payload is too large",
                "application/problem+json",
                [typeof(ApiProblemDetails)],
                cancellationToken);
        await AddResponseSchemasAsync(
            operation,
            context,
            "500",
            "Unexpected server error",
            "application/problem+json",
            [typeof(ApiProblemDetails)],
            cancellationToken);

        AddLocalizationResponseHeaders(operation);
    }

    private static bool IsApiOperation(string? relativePath)
    {
        if (relativePath is null) return false;
        var path = relativePath.TrimStart('/');
        return path.Equals("api", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("api/", StringComparison.OrdinalIgnoreCase);
    }

    private static OpenApiSchema CreateLocaleSchema()
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            MinLength = Locale.EnglishCode.Length,
            MaxLength = Locale.EnglishCode.Length,
            Pattern = "^(en|ru)$",
            Enum = new List<JsonNode>()
        };
        foreach (var locale in Locale.SupportedCodes) schema.Enum.Add(locale);
        return schema;
    }

    private static void AddLocalizationResponseHeaders(OpenApiOperation operation)
    {
        if (operation.Responses is null) return;

        foreach (var responseEntry in operation.Responses)
        {
            if (responseEntry.Key == "406") continue;
            if (responseEntry.Value is not OpenApiResponse response)
                throw new OpenApiException($"Response {responseEntry.Key} cannot define headers");

            response.Headers ??= new Dictionary<string, IOpenApiHeader>();
            response.Headers.TryAdd(HeaderNames.ContentLanguage, new OpenApiHeader
            {
                Description = "Locale used for the response.",
                Required = true,
                Schema = CreateLocaleSchema()
            });
        }
    }

    private static async Task AddResponseSchemasAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        string statusCode,
        string description,
        string contentType,
        IReadOnlyCollection<Type> types,
        CancellationToken cancellationToken)
    {
        var schemas = new List<IOpenApiSchema>();
        foreach (var type in types)
        {
            schemas.Add(await CreateSchemaReferenceAsync(context, type, cancellationToken));
        }

        operation.Responses ??= new OpenApiResponses();
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = description };
            operation.Responses.Add(statusCode, response);
        }

        var content = response.Content;
        if (content is null)
        {
            if (response is not OpenApiResponse concreteResponse)
                throw new OpenApiException($"Response {statusCode} cannot define content");

            content = new Dictionary<string, OpenApiMediaType>();
            concreteResponse.Content = content;
        }

        if (!content.TryGetValue(contentType, out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            content.Add(contentType, mediaType);
        }

        if (mediaType.Schema is null)
        {
            mediaType.Schema = CreateOneOf(schemas);
            return;
        }

        if (mediaType.Schema is OpenApiSchema { OneOf: not null, Discriminator: null } oneOfSchema)
        {
            foreach (var schema in schemas) oneOfSchema.OneOf.Add(schema);
            return;
        }

        schemas.Insert(0, mediaType.Schema);
        mediaType.Schema = CreateOneOf(schemas);
    }

    private static OpenApiSchema CreateOneOf(List<IOpenApiSchema> schemas) => new()
    {
        OneOf = schemas
    };

    private static async Task<IOpenApiSchema> CreateSchemaReferenceAsync(
        OpenApiOperationTransformerContext context,
        Type type,
        CancellationToken cancellationToken)
    {
        var document = context.Document ?? throw new OpenApiException("Document cannot be null");
        var schema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);
        if (type == typeof(ApiProblemDetails) || type == typeof(ApiValidationProblemDetails))
        {
            schema.Properties?.Remove("extensions");
            schema.Required?.Remove("extensions");
        }

        var schemaId = schema.GetOpenApiSchemaId();
        if (string.IsNullOrEmpty(schemaId))
            throw new OpenApiException($"Schema id for {type.FullName} cannot be null or empty");

        document.Components?.Schemas?.TryAdd(schemaId, schema);
        document.Workspace?.RegisterComponentForDocument(document, schema, schemaId);
        return new OpenApiSchemaReference(schemaId, document);
    }
}
