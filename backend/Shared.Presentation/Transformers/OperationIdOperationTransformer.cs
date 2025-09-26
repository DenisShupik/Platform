using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class OperationIdOperationTransformer : IOpenApiOperationTransformer
{
    private const string Suffix = "Async";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (
            context.Description.ActionDescriptor.EndpointMetadata.FirstOrDefault() is not MethodInfo methodInfo ||
            !methodInfo.Name.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase)
        )
            return Task.CompletedTask;

        var name = methodInfo.Name.AsSpan(..^5);
        operation.OperationId = string.Create(
            name.Length,
            name,
            (destination, source) =>
            {
                destination[0] = char.ToLowerInvariant(source[0]);
                source[1..].CopyTo(destination[1..]);
            });
        return Task.CompletedTask;
    }
}