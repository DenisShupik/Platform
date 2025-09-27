using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class JsonPolymorphicDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is null) return Task.CompletedTask;
        foreach (var schema in document.Components.Schemas.Values)
        {
            if (schema.Discriminator == null) continue;
            // TODO: mapping можно вытащить из AnyOf
            if (schema.Discriminator.Mapping == null) continue;
            foreach (var e in schema.Discriminator.Mapping.Values)
            {
                if (e.Reference.Id == null) continue;
                if (!document.Components.Schemas.TryGetValue(e.Reference.Id, out var maybeValueSchema)) continue;
                if (maybeValueSchema is not OpenApiSchema valueSchema) continue;
                valueSchema.Required ??= new HashSet<string>();
                valueSchema.Required.Add("$type");
            }
        }

        return Task.CompletedTask;
    }
}