using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Extensions;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Transformers;

public sealed class EnhanceOperationTransformer : IOpenApiOperationTransformer
{
    private const string Suffix = "Async";

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.FirstOrDefault() is not MethodInfo methodInfo)
                throw new OpenApiException("Minimal api handler must be a named method");

            if (!methodInfo.Name.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
                throw new OpenApiException("Minimal api handler name must end with " + Suffix);

            var name = methodInfo.Name.AsSpan(..^5);
            operation.OperationId = string.Create(
                name.Length,
                name,
                (destination, source) =>
                {
                    destination[0] = char.ToLowerInvariant(source[0]);
                    source[1..].CopyTo(destination[1..]);
                });

            var errorResponses = context.Description.ActionDescriptor.EndpointMetadata
                .Where(r => r is ProducesResponseTypeMetadata { StatusCode: >= 400 and <= 499 })
                .Select(r => (r as ProducesResponseTypeMetadata)!)
                .GroupBy(r => r.StatusCode);

            if (operation.Responses != null)
                foreach (var group in errorResponses.Where(e => e.Count() > 1))
                {
                    var list = new List<IOpenApiSchema>();
                    foreach (var type in group.Where(e => e.Type is not null).Select(e => e.Type!))
                    {
                        if (!type.IsSubclassOf(typeof(Error)))
                            throw new OpenApiException($"Invalid operation error response type: {type}");
                        var schema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);

                        var schemaId = schema.GetOpenApiSchemaId();

                        if (schema.Properties != null)
                            foreach (var key in schema.Properties.Keys)
                            {
                                schema.Properties.TryGetValue(key, out var value);
                                if (value is not OpenApiSchema propSchema) continue;
                                var propSchemaId = propSchema.TryGetOpenApiSchemaId();
                                if (string.IsNullOrEmpty(propSchemaId)) continue;
                                context.Document?.Components?.Schemas?.TryAdd(propSchemaId, propSchema);
                                context.Document?.Workspace?.RegisterComponentForDocument(context.Document, propSchema,
                                    propSchemaId);
                                var refPropSchema = new OpenApiSchemaReference(propSchemaId, context.Document);
                                schema.Properties[key] = refPropSchema;
                            }

                        if (schema.AnyOf != null)
                            for (var i = 0; i < schema.AnyOf.Count; i++)
                            {
                                if (schema.AnyOf[i] is not OpenApiSchema propSchema) continue;
                                var propSchemaId = propSchema.TryGetOpenApiSchemaId();
                                if (string.IsNullOrEmpty(propSchemaId)) continue;
                                context.Document?.Components?.Schemas?.TryAdd(propSchemaId, propSchema);
                                context.Document?.Workspace?.RegisterComponentForDocument(context.Document, propSchema,
                                    propSchemaId);

                                var refPropSchema = new OpenApiSchemaReference(propSchemaId, context.Document);
                                schema.AnyOf[i] = refPropSchema;
                            }

                        context.Document?.Components?.Schemas?.TryAdd(schemaId, schema);
                        context.Document?.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);

                        list.Add(new OpenApiSchemaReference(schemaId, context.Document));
                    }

                    if (!operation.Responses.TryGetValue(group.Key.ToString(), out var response))
                        throw new OpenApiException($"Operation error response for {group.Key} not found");

                    if (response.Content == null ||
                        !response.Content.TryGetValue("application/json", out var contentJson))
                        throw new OpenApiException("Operation error response content for application/json not found");

                    contentJson.Schema = new OpenApiSchema
                    {
                        OneOf = list,
                        Discriminator = new OpenApiDiscriminator { PropertyName = "$type" }
                    };
                }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}