using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Errors;

namespace Shared.Presentation.Transformers;

public sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (type != typeof(ApiProblemDetails) && type != typeof(ApiValidationProblemDetails))
            return Task.CompletedTask;

        schema.Properties?.Remove("extensions");
        schema.Required?.Remove("extensions");
        return Task.CompletedTask;
    }
}
