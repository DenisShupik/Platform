using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Transformers;

public sealed class ErrorSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string DiscriminatorPropertyName = "$type";

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!typeof(Error).IsAssignableFrom(type) || type == typeof(Error) || schema.Properties is null ||
            !schema.Properties.TryGetValue(DiscriminatorPropertyName, out var discriminatorSchema) ||
            discriminatorSchema is not OpenApiSchema concreteDiscriminatorSchema)
            return Task.CompletedTask;

        concreteDiscriminatorSchema.Type = JsonSchemaType.String;
        concreteDiscriminatorSchema.Const = type.Name;

        schema.Required ??= new HashSet<string>();
        schema.Required.Add(DiscriminatorPropertyName);

        return Task.CompletedTask;
    }
}
