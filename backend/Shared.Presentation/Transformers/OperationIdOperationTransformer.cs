using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Errors;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class OperationIdOperationTransformer : IOpenApiOperationTransformer
{
    private const string Suffix = "Async";

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (
            context.Description.ActionDescriptor.EndpointMetadata.FirstOrDefault() is not MethodInfo methodInfo ||
            !methodInfo.Name.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase)
        )
            return;

        var name = methodInfo.Name.AsSpan(..^5);
        operation.OperationId = string.Create(
            name.Length,
            name,
            (destination, source) =>
            {
                destination[0] = char.ToLowerInvariant(source[0]);
                source[1..].CopyTo(destination[1..]);
            });


        var byStatus = context.Description.ActionDescriptor.EndpointMetadata
            .Where(r => r is ProducesResponseTypeMetadata { StatusCode: >= 400 and <= 499 })
            .Select(r => (r as ProducesResponseTypeMetadata)!)
            .GroupBy(r => r.StatusCode);

        if (operation.Responses != null)
            foreach (var group in byStatus.Where(e => e.Count() > 1))
            {
                var list = new Dictionary<string, OpenApiSchemaReference>();
                foreach (var type in group.Where(e => e.Type is not null).Select(e => e.Type!))
                {
                    if (!type.IsSubclassOf(typeof(Error)))
                        throw new Exception($"Invalid operation error response type: {type}");
                    var schema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);

                    var schemaId = schema.GetOpenApiSchemaId();

                    if (schema.Properties != null)
                        foreach (var key in schema.Properties.Keys)
                        {
                            schema.Properties.TryGetValue(key, out var value);
                            if (value is not OpenApiSchema propShema) continue;
                            var propSchemaId = propShema.TryGetOpenApiSchemaId();
                            if (string.IsNullOrEmpty(propSchemaId)) continue;
                            var refPropSchema = new OpenApiSchemaReference(propSchemaId, context.Document);
                            schema.Properties[key] = refPropSchema;
                        }

                    context.Document?.Components?.Schemas?.TryAdd(schemaId, schema);
                    context.Document?.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
                    list.Add(schemaId, new OpenApiSchemaReference(schemaId, context.Document));
                }

                if (!operation.Responses.TryGetValue(group.Key.ToString(), out var response))
                    throw new KeyNotFoundException($"Operation error response for {group.Key} not found");

                if (response.Content == null || !response.Content.TryGetValue("application/json", out var contentJson))
                    throw new KeyNotFoundException("Operation error response content for application/json not found");

                contentJson.Schema = new OpenApiSchema
                {
                    OneOf = list.Values.Select(IOpenApiSchema (e) => e).ToList(),
                    Discriminator = new OpenApiDiscriminator { PropertyName = "$type", Mapping = list }
                };
            }
    }
}