using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class JsonPolymorphicDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var items =
            document.Components?.Schemas?
                .Where(e => e.Value.Discriminator != null)
                .Select(e => e.Value.Discriminator!);

        if (items is null) return Task.CompletedTask;
        foreach (var item in items)
        {
            if (item.Mapping == null) continue;
            foreach (var e in item.Mapping.Values)
            {
                if (e.Reference.Id == null) continue;
                if (!(document.Components?.Schemas?.TryGetValue(e.Reference.Id, out var maybeSchema) ?? false)) continue;
                if (maybeSchema is not OpenApiSchema schema) continue;
                schema.Required ??= new HashSet<string>();
                schema.Required.Add("$type");
            }
        }

        return Task.CompletedTask;
    }
}