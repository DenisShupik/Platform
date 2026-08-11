using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class JsonPolymorphicDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is null) return Task.CompletedTask;

        foreach (var schema in document.Components.Schemas.Values.OfType<OpenApiSchema>())
        {
            if (schema.Discriminator is not { PropertyName.Length: > 0 } discriminator) continue;

            RequireProperty(schema, discriminator.PropertyName);

            var derivedSchemas = discriminator.Mapping?.Values
                                     ?? schema.AnyOf?.OfType<OpenApiSchemaReference>().ToArray();
            if (derivedSchemas is null) continue;

            foreach (var derivedSchemaReference in derivedSchemas)
            {
                var referenceId = derivedSchemaReference.Reference.Id;
                if (referenceId is null ||
                    !document.Components.Schemas.TryGetValue(referenceId, out var derivedSchema) ||
                    derivedSchema is not OpenApiSchema concreteDerivedSchema)
                    continue;

                RequireProperty(concreteDerivedSchema, discriminator.PropertyName);
            }
        }

        return Task.CompletedTask;
    }

    private static void RequireProperty(OpenApiSchema schema, string propertyName)
    {
        schema.Required ??= new HashSet<string>(StringComparer.Ordinal);
        schema.Required.Add(propertyName);
    }
}
