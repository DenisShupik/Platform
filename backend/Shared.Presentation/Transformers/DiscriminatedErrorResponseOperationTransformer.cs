using System.Globalization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Transformers;

/// <summary>
/// Refines the framework's same-status <c>anyOf</c> response into a
/// discriminated <c>oneOf</c> when every alternative is a domain error.
/// Domain errors are mutually exclusive because their <c>$type</c> property
/// is required and constrained with a distinct <c>const</c> value.
/// </summary>
public sealed class DiscriminatedErrorResponseOperationTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var errorGroups = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IProducesResponseTypeMetadata>()
            .Where(static metadata =>
                metadata.Type is not null &&
                metadata.Type != typeof(Error) &&
                typeof(Error).IsAssignableFrom(metadata.Type))
            .GroupBy(static metadata => metadata.StatusCode);

        foreach (var group in errorGroups)
        {
            var errorTypes = group
                .Select(static metadata => metadata.Type!)
                .Distinct()
                .ToArray();
            if (errorTypes.Length < 2) continue;

            var statusCode = group.Key.ToString(CultureInfo.InvariantCulture);
            if (operation.Responses?.TryGetValue(statusCode, out var response) != true ||
                response is not OpenApiResponse { Content: not null } concreteResponse ||
                !concreteResponse.Content.TryGetValue("application/json", out var mediaType) ||
                mediaType is not OpenApiMediaType concreteMediaType)
                continue;

            var schemas = new List<IOpenApiSchema>(errorTypes.Length);
            foreach (var errorType in errorTypes)
                schemas.Add(await context.GetOrCreateSchemaAsync(errorType, null, cancellationToken));

            concreteMediaType.Schema = new OpenApiSchema
            {
                OneOf = schemas,
                Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
            };
        }
    }
}
