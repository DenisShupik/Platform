using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Errors;

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

        AddOrReplaceAcceptLanguageParameter(operation);

        await AddResponseSchemasAsync(
            operation,
            context,
            "400",
            "Invalid request",
            "application/problem+json",
            BadRequestTypes,
            SchemaComposition.AnyOf,
            cancellationToken);
        await AddResponseSchemasAsync(
            operation,
            context,
            "406",
            "A supported Accept-Language header is required",
            "application/json",
            LocaleErrorTypes,
            SchemaComposition.DiscriminatedOneOf,
            cancellationToken);
        if (operation.RequestBody is not null)
            await AddResponseSchemasAsync(
                operation,
                context,
                "413",
                "Request payload is too large",
                "application/problem+json",
                [typeof(ApiProblemDetails)],
                SchemaComposition.AnyOf,
                cancellationToken);
        await AddResponseSchemasAsync(
            operation,
            context,
            "500",
            "Unexpected server error",
            "application/problem+json",
            [typeof(ApiProblemDetails)],
            SchemaComposition.AnyOf,
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

    private static void AddOrReplaceAcceptLanguageParameter(OpenApiOperation operation)
    {
        operation.Parameters ??= [];

        var parameter = new OpenApiParameter
        {
            Name = HeaderNames.AcceptLanguage,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Requested response locale.",
            Schema = CreateLocaleSchema()
        };

        var matchingIndexes = operation.Parameters
            .Select((candidate, index) => (candidate, index))
            .Where(item => item.candidate is OpenApiParameter
            {
                In: ParameterLocation.Header,
                Name: not null
            } candidate && candidate.Name.Equals(HeaderNames.AcceptLanguage, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.index)
            .ToArray();

        if (matchingIndexes.Length == 0)
        {
            operation.Parameters.Add(parameter);
            return;
        }

        operation.Parameters[matchingIndexes[0]] = parameter;
        for (var index = matchingIndexes.Length - 1; index > 0; index--)
            operation.Parameters.RemoveAt(matchingIndexes[index]);
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
            response.Headers[HeaderNames.ContentLanguage] = new OpenApiHeader
            {
                Description = "Locale used for the response.",
                Required = true,
                Schema = CreateLocaleSchema()
            };
        }
    }

    private static async Task AddResponseSchemasAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        string statusCode,
        string description,
        string contentType,
        IReadOnlyCollection<Type> types,
        SchemaComposition composition,
        CancellationToken cancellationToken)
    {
        var schemas = new List<IOpenApiSchema>();
        foreach (var type in types.Distinct())
            schemas.Add(await context.GetOrCreateSchemaAsync(type, null, cancellationToken));

        operation.Responses ??= new OpenApiResponses();
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = description };
            operation.Responses.Add(statusCode, response);
        }

        if (response is not OpenApiResponse concreteResponse)
            throw new OpenApiException($"Response {statusCode} cannot define content");

        concreteResponse.Content ??= new Dictionary<string, IOpenApiMediaType>();
        if (!concreteResponse.Content.TryGetValue(contentType, out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            concreteResponse.Content.Add(contentType, mediaType);
        }

        if (mediaType is not OpenApiMediaType concreteMediaType)
            throw new OpenApiException(
                $"Response {statusCode} content for {contentType} must be inline");

        concreteMediaType.Schema = MergeSchemas(concreteMediaType.Schema, schemas, composition);
    }

    private static IOpenApiSchema MergeSchemas(
        IOpenApiSchema? existingSchema,
        IEnumerable<IOpenApiSchema> additionalSchemas,
        SchemaComposition composition)
    {
        var schemas = new List<IOpenApiSchema>();
        if (existingSchema is OpenApiSchema existingComposite &&
            TryGetCompositeSchemas(existingComposite, composition, out var existingSchemas))
            schemas.AddRange(existingSchemas);
        else if (existingSchema is not null)
            schemas.Add(existingSchema);

        foreach (var schema in additionalSchemas)
            if (!schemas.Any(candidate => HasSameIdentity(candidate, schema)))
                schemas.Add(schema);

        if (schemas.Count == 0)
            throw new OpenApiException("At least one response schema is required");
        if (schemas.Count == 1)
            return schemas[0];

        return composition switch
        {
            SchemaComposition.AnyOf => new OpenApiSchema { AnyOf = schemas },
            SchemaComposition.DiscriminatedOneOf => new OpenApiSchema
            {
                OneOf = schemas,
                Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
            },
            _ => throw new OpenApiException($"Unsupported schema composition {composition}")
        };
    }

    private static bool TryGetCompositeSchemas(
        OpenApiSchema schema,
        SchemaComposition composition,
        out IList<IOpenApiSchema> schemas)
    {
        if (composition == SchemaComposition.AnyOf && schema.AnyOf is not null)
        {
            schemas = schema.AnyOf;
            return true;
        }

        if (composition == SchemaComposition.DiscriminatedOneOf &&
            schema.OneOf is not null &&
            schema.Discriminator?.PropertyName == "$type")
        {
            schemas = schema.OneOf;
            return true;
        }

        schemas = Array.Empty<IOpenApiSchema>();
        return false;
    }

    private static bool HasSameIdentity(IOpenApiSchema left, IOpenApiSchema right)
    {
        if (ReferenceEquals(left, right)) return true;
        return left is OpenApiSchemaReference leftReference &&
               right is OpenApiSchemaReference rightReference &&
               leftReference.Reference.ReferenceV3 == rightReference.Reference.ReferenceV3;
    }

    private enum SchemaComposition : byte
    {
        AnyOf,
        DiscriminatedOneOf
    }
}
