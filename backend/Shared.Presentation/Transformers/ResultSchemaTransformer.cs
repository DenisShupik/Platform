using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions.Results;

namespace Shared.Presentation.Transformers;

public sealed class ResultSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsGenericType) return;

        var typeDefinition = type.GetGenericTypeDefinition();

        if (!typeof(IResult).IsAssignableFrom(typeDefinition)) return;

        var valueType = type.GetGenericArguments()[0];
        var errorTypes = type.GetGenericArguments().Skip(1);

        var valueSchema = await context.GetOrCreateSchemaAsync(valueType, null, cancellationToken);

        var errorSchemas = new List<IOpenApiSchema>();
        foreach (var errorType in errorTypes.Distinct())
            errorSchemas.Add(await context.GetOrCreateSchemaAsync(errorType, null, cancellationToken));

        var errorsSchema = errorSchemas.Count switch
        {
            0 => throw new OpenApiException($"Result type {type.FullName} must declare at least one error type"),
            1 => errorSchemas[0],
            _ => new OpenApiSchema
            {
                OneOf = errorSchemas,
                Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
            }
        };

        schema.Type = JsonSchemaType.Object;
        schema.Format = null;
        schema.Properties = null;
        schema.Required = null;
        schema.AllOf = null;
        schema.AnyOf = null;
        schema.Items = null;
        schema.AdditionalProperties = null;
        schema.OneOf =
        [
            new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema> { ["value"] = valueSchema },
                Required = new HashSet<string> { "value" }
            },
            new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema> { ["error"] = errorsSchema },
                Required = new HashSet<string> { "error" }
            }
        ];
    }
}
