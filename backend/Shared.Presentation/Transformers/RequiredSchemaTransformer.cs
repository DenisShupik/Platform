using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class RequiredSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly NullabilityInfoContext _nullabilityInfoContext = new();

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Properties == null) return Task.CompletedTask;

        schema.Required ??= new HashSet<string>(StringComparer.Ordinal);

        foreach (var jsonPropertyInfo in context.JsonTypeInfo.Properties)
        {
            if (jsonPropertyInfo.AttributeProvider is not PropertyInfo propertyInfo)
                continue;

            var nullability = _nullabilityInfoContext.Create(propertyInfo);

            if (nullability.WriteState == NullabilityState.NotNull ||
                (nullability.WriteState == NullabilityState.Unknown && propertyInfo.PropertyType.IsAbstract))
            {
                schema.Required.Add(jsonPropertyInfo.Name);
            }
            else if (nullability is { WriteState: NullabilityState.Unknown, ReadState: NullabilityState.NotNull })
            {
                if (schema.Properties.TryGetValue(jsonPropertyInfo.Name, out var value) &&
                    value is OpenApiSchema schemaValue)
                {
                    schemaValue.ReadOnly = true;
                    if (schemaValue.Type != null) schemaValue.Type = schemaValue.Type.Value & ~JsonSchemaType.Null;
                }

                schema.Required.Add(jsonPropertyInfo.Name);
            }
        }

        return Task.CompletedTask;
    }
}