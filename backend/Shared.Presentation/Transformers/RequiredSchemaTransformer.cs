using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class RequiredSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly ConcurrentDictionary<PropertyInfo, PropertyRequirement> RequirementCache = new();

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Properties is null) return Task.CompletedTask;

        foreach (var jsonProperty in context.JsonTypeInfo.Properties)
        {
            if (jsonProperty.AttributeProvider is not PropertyInfo property) continue;

            var requirement = RequirementCache.GetOrAdd(property, GetPropertyRequirement);
            if (requirement == PropertyRequirement.Optional) continue;

            schema.Required ??= new HashSet<string>(StringComparer.Ordinal);
            schema.Required.Add(jsonProperty.Name);

            if (requirement != PropertyRequirement.ReadOnly ||
                !schema.Properties.TryGetValue(jsonProperty.Name, out var propertySchema))
                continue;

            schema.Properties[jsonProperty.Name] = CreateReadOnlySchema(propertySchema);
        }

        return Task.CompletedTask;
    }

    private static PropertyRequirement GetPropertyRequirement(PropertyInfo property)
    {
        var nullability = new NullabilityInfoContext().Create(property);
        if (nullability.WriteState == NullabilityState.NotNull ||
            (nullability.WriteState == NullabilityState.Unknown && property.PropertyType.IsAbstract))
            return PropertyRequirement.Required;

        return nullability is { WriteState: NullabilityState.Unknown, ReadState: NullabilityState.NotNull }
            ? PropertyRequirement.ReadOnly
            : PropertyRequirement.Optional;
    }

    private static IOpenApiSchema CreateReadOnlySchema(IOpenApiSchema schema)
    {
        switch (schema)
        {
            case OpenApiSchema concreteSchema:
            {
                var localSchema = (OpenApiSchema)concreteSchema.CreateShallowCopy();
                localSchema.ReadOnly = true;
                if (localSchema.Type is not null)
                    localSchema.Type &= ~JsonSchemaType.Null;
                return localSchema;
            }
            case OpenApiSchemaReference referenceSchema:
            {
                var localReference = (OpenApiSchemaReference)referenceSchema.CreateShallowCopy();
                localReference.ReadOnly = true;
                return localReference;
            }
            default:
                throw new OpenApiException($"Schema type {schema.GetType().FullName} cannot be marked read-only");
        }
    }

    private enum PropertyRequirement : byte
    {
        Optional,
        Required,
        ReadOnly
    }
}
