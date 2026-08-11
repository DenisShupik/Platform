using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Extensions;

public static class OpenApiTransformerContextExtensions
{
    public static async Task<OpenApiSchemaReference> GetOrAddSchemaReferenceAsync(
        this OpenApiOperationTransformerContext context,
        Type type,
        CancellationToken cancellationToken)
    {
        var document = context.Document ?? throw new OpenApiException("Document cannot be null");
        var schema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);
        return document.GetOrAddSchemaReference(schema);
    }

    public static async Task<OpenApiSchemaReference> GetOrAddSchemaReferenceAsync(
        this OpenApiSchemaTransformerContext context,
        Type type,
        CancellationToken cancellationToken)
    {
        var document = context.Document ?? throw new OpenApiException("Document cannot be null");
        var schema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);
        return document.GetOrAddSchemaReference(schema);
    }
}
