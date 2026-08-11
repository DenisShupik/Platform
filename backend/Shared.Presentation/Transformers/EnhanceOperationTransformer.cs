using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions.Errors;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class EnhanceOperationTransformer : IOpenApiOperationTransformer
{
    private const string Suffix = "Async";
    private const string JsonContentType = "application/json";

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var methodInfo = metadata.OfType<MethodInfo>().FirstOrDefault()
                         ?? throw new OpenApiException("Minimal API handler must be a named method");

        if (!methodInfo.Name.EndsWith(Suffix, StringComparison.Ordinal) || methodInfo.Name.Length == Suffix.Length)
            throw new OpenApiException($"Minimal API handler name must end with {Suffix}");

        var name = methodInfo.Name.AsSpan(..^Suffix.Length);
        operation.OperationId = string.Create(
            name.Length,
            name,
            static (destination, source) =>
            {
                destination[0] = char.ToLowerInvariant(source[0]);
                source[1..].CopyTo(destination[1..]);
            });

        var errorResponseGroups = metadata
            .OfType<ProducesResponseTypeMetadata>()
            .Where(response => response.StatusCode is >= 400 and <= 499 && response.Type is not null)
            .GroupBy(response => response.StatusCode)
            .Where(group => group.Select(response => response.Type).Distinct().Skip(1).Any());

        foreach (var group in errorResponseGroups)
        {
            var schemas = new List<IOpenApiSchema>();
            foreach (var type in group.Select(response => response.Type!).Distinct())
            {
                if (!typeof(Error).IsAssignableFrom(type))
                    throw new OpenApiException($"Invalid operation error response type: {type}");

                schemas.Add(await context.GetOrAddSchemaReferenceAsync(type, cancellationToken));
            }

            if (operation.Responses is null ||
                !operation.Responses.TryGetValue(group.Key.ToString(), out var response))
                throw new OpenApiException($"Operation error response for {group.Key} not found");

            if (response.Content is null ||
                !response.Content.TryGetValue(JsonContentType, out var jsonContent))
                throw new OpenApiException(
                    $"Operation error response content for {JsonContentType} not found");

            jsonContent.Schema = new OpenApiSchema
            {
                OneOf = schemas,
                Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
            };
        }
    }
}
