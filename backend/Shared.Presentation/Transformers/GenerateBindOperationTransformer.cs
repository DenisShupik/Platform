using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class GenerateBindOperationTransformer : IOpenApiOperationTransformer
{
    private const string GenerateBindAttributeFullName =
        "Shared.Presentation.Generator.Attributes.GenerateBindAttribute";

    private static readonly ConcurrentDictionary<Type, BindTypeMetadata> MetadataCache = new();

    private enum SourceLocation : byte
    {
        Path,
        Query,
        Header,
        Body
    }

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var parameterMetadata in context.Description.ActionDescriptor.EndpointMetadata
                     .OfType<IParameterBindingMetadata>())
        {
            var metadata = MetadataCache.GetOrAdd(
                parameterMetadata.ParameterInfo.ParameterType,
                CreateBindTypeMetadata);
            if (!metadata.IsGenerated) continue;

            foreach (var property in metadata.Properties)
            {
                var schema = await context.GetOrCreateSchemaAsync(
                    property.PropertyType,
                    null,
                    cancellationToken);

                if (property.Location == SourceLocation.Body)
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Content = new Dictionary<string, IOpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = schema
                            }
                        }
                    };
                    continue;
                }

                var parameterSchema = CreateParameterSchema(schema, property.IsNullable);
                var required = property.Location == SourceLocation.Path || !property.IsNullable;
                if (property.HasDefault)
                {
                    SetDefault(parameterSchema, CreateDefault(property.DefaultValue));
                    required = false;
                }

                AddOrReplaceParameter(operation, new OpenApiParameter
                {
                    Name = property.Name,
                    In = MapToOpenApiParameterLocation(property.Location),
                    Required = required,
                    Schema = parameterSchema
                });
            }
        }
    }

    private static BindTypeMetadata CreateBindTypeMetadata(Type type)
    {
        var isGenerated = type.GetCustomAttributes(inherit: false)
            .Any(attribute => attribute.GetType().FullName == GenerateBindAttributeFullName);
        if (!isGenerated) return new BindTypeMetadata(false, []);

        var defaultsContainer = type.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.NonPublic);
        var nullabilityContext = new NullabilityInfoContext();
        var properties = new List<BindPropertyMetadata>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var (location, name) = GetParameterLocationAndName(property);
            if (location is null || name is null) continue;

            object? defaultValue = null;
            var hasDefault = defaultsContainer is not null &&
                             TryGetDefaultsValue(defaultsContainer, property.Name, out defaultValue);
            properties.Add(new BindPropertyMetadata(
                property.PropertyType,
                location.Value,
                name,
                IsNullableProperty(property, nullabilityContext),
                hasDefault,
                hasDefault ? defaultValue : null));
        }

        return new BindTypeMetadata(true, properties);
    }

    private static IOpenApiSchema CreateParameterSchema(
        OpenApiSchema schema,
        bool isNullable)
    {
        if (!isNullable)
            return schema.CreateShallowCopy();

        if (schema.OneOf is not null)
        {
            var nonNullSchemas = schema.OneOf.Where(candidate => !IsNullSchema(candidate)).ToArray();
            if (nonNullSchemas.Length == 1) return nonNullSchemas[0];
            if (nonNullSchemas.Length > 1) return new OpenApiSchema { OneOf = nonNullSchemas };
        }

        var localSchema = (OpenApiSchema)schema.CreateShallowCopy();
        if (localSchema.Type is not null)
            localSchema.Type &= ~JsonSchemaType.Null;
        return localSchema;
    }

    private static bool IsNullSchema(IOpenApiSchema schema) =>
        schema is OpenApiSchema { Type: JsonSchemaType.Null };

    private static void AddOrReplaceParameter(OpenApiOperation operation, OpenApiParameter parameter)
    {
        operation.Parameters ??= [];
        for (var index = 0; index < operation.Parameters.Count; index++)
        {
            if (operation.Parameters[index] is not OpenApiParameter existing ||
                existing.In != parameter.In ||
                !string.Equals(existing.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            operation.Parameters[index] = parameter;
            return;
        }

        operation.Parameters.Add(parameter);
    }

    private static (SourceLocation? location, string? name) GetParameterLocationAndName(PropertyInfo property)
    {
        if (property.GetCustomAttribute<FromRouteAttribute>() is { } fromRoute)
            return (SourceLocation.Path, fromRoute.Name ?? property.Name.ToCamelCase());

        if (property.GetCustomAttribute<FromQueryAttribute>() is { } fromQuery)
            return (SourceLocation.Query, fromQuery.Name ?? property.Name.ToCamelCase());

        if (property.GetCustomAttribute<FromHeaderAttribute>() is { } fromHeader)
            return (SourceLocation.Header, fromHeader.Name ?? property.Name.ToCamelCase());

        if (property.GetCustomAttribute<FromBodyAttribute>() is not null)
            return (SourceLocation.Body, property.Name);

        return (null, null);
    }

    private static bool TryGetDefaultsValue(Type defaultsContainer, string memberName, out object? value)
    {
        var field = defaultsContainer.GetField(
            memberName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field is not null)
        {
            value = field.GetValue(null);
            return true;
        }

        var property = defaultsContainer.GetProperty(
            memberName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (property is not null)
        {
            value = property.GetValue(null);
            return true;
        }

        value = null;
        return false;
    }

    private static JsonNode CreateDefault(object? value)
    {
        if (value is null)
            throw new OpenApiException("A declared request default cannot be null");

        if (value is bool boolean) return JsonValue.Create(boolean);

        var type = value.GetType();
        if (type.IsGenericType)
        {
            var typeDefinition = type.GetGenericTypeDefinition();
            if (typeDefinition == typeof(SortCriteria<>))
                return CreateSortCriteriaDefault(type, value);
            if (typeDefinition == typeof(SortCriteriaList<>))
                return CreateSortCriteriaListDefault(type, value);
        }

        var vogenInterface = type.GetInterfaces()
            .FirstOrDefault(candidate =>
                candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() == typeof(IVogen<,>));
        if (vogenInterface is null)
            throw new OpenApiException($"Default value type {type.FullName} is not supported");

        var valueProperty = vogenInterface.GetProperty(nameof(IVogen<,>.Value))
                            ?? throw new OpenApiException(
                                $"Vogen type {type.FullName} does not expose its primitive value");
        var primitiveValue = valueProperty.GetValue(value)
                             ?? throw new OpenApiException(
                                 $"Default value for Vogen type {type.FullName} cannot be null");

        return JsonValue.Create(primitiveValue)
               ?? throw new OpenApiException(
                   $"Default value for Vogen type {type.FullName} cannot be represented as JSON");
    }

    private static JsonNode CreateSortCriteriaDefault(Type type, object value)
    {
        var field = type.GetProperty(nameof(SortCriteria<>.Field))?.GetValue(value)?.ToString()?.ToCamelCase()
                    ?? throw new OpenApiException($"Sort default {type.FullName} has no field");
        var order = type.GetProperty(nameof(SortCriteria<>.Order))?.GetValue(value) as SortOrderType?
                    ?? throw new OpenApiException($"Sort default {type.FullName} has no order");

        return JsonValue.Create(order == SortOrderType.Descending ? "-" + field : field);
    }

    private static JsonNode CreateSortCriteriaListDefault(Type type, object value)
    {
        var enumType = type.GetGenericArguments()[0];
        var itemType = typeof(SortCriteria<>).MakeGenericType(enumType);
        var toArray = type.GetMethod("ToArray", Type.EmptyTypes)
                      ?? throw new OpenApiException($"Sort list {type.FullName} does not expose ToArray()");
        var values = toArray.Invoke(value, null) as Array
                     ?? throw new OpenApiException($"Sort list {type.FullName} did not produce an array");
        var result = new JsonArray();
        foreach (var item in values)
            result.Add(CreateSortCriteriaDefault(itemType, item!));
        return result;
    }

    private static void SetDefault(IOpenApiSchema schema, JsonNode value)
    {
        switch (schema)
        {
            case OpenApiSchema concreteSchema:
                concreteSchema.Default = value;
                break;
            case OpenApiSchemaReference referenceSchema:
                referenceSchema.Default = value;
                break;
            default:
                throw new OpenApiException($"Schema type {schema.GetType().FullName} cannot define a default");
        }
    }

    private static ParameterLocation MapToOpenApiParameterLocation(SourceLocation location) => location switch
    {
        SourceLocation.Path => ParameterLocation.Path,
        SourceLocation.Query => ParameterLocation.Query,
        SourceLocation.Header => ParameterLocation.Header,
        _ => throw new OpenApiException("Not a parameter location")
    };

    private static bool IsNullableProperty(PropertyInfo property, NullabilityInfoContext context)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) is not null) return true;
        return context.Create(property).ReadState == NullabilityState.Nullable;
    }

    private sealed record BindTypeMetadata(bool IsGenerated, IReadOnlyList<BindPropertyMetadata> Properties);

    private sealed record BindPropertyMetadata(
        Type PropertyType,
        SourceLocation Location,
        string Name,
        bool IsNullable,
        bool HasDefault,
        object? DefaultValue);
}
