using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class OperationIdOperationTransformer : IOpenApiOperationTransformer
{
    private const string Suffix = "Async";

    public Task TransformAsync(
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

        return Task.CompletedTask;
    }
}
