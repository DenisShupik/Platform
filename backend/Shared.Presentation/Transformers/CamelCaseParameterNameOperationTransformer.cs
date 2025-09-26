using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class CamelCaseParameterNameOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var openApiParameters = operation.Parameters;
        if (openApiParameters == null) return Task.CompletedTask;
        foreach (var parameter in openApiParameters)
        {
            if (parameter is not OpenApiParameter schema || schema.Name == null) continue;
            schema.Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(schema.Name);
        }

        return Task.CompletedTask;
    }
}