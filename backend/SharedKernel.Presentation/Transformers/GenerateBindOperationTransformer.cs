using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Presentation.Extensions;

namespace SharedKernel.Presentation.Transformers;

public sealed class GenerateBindOperationTransformer : IOpenApiOperationTransformer
{
    private const string GenerateBindAttributeFullName = "SharedKernel.Presentation.Generator.GenerateBindAttribute";

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var parameter in context.Description.ActionDescriptor.EndpointMetadata)
        {
            if (parameter is not IParameterBindingMetadata p) continue;

            var type = p.ParameterInfo.ParameterType;

            if (!HasGenerateBindAttribute(type)) continue;

            var defaultsContainer = type.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var (location, name) = GetParameterLocationAndName(prop);
                if (location == null) continue;

                var schema = await context.GetOrCreateSchemaAsync(prop.PropertyType, null, cancellationToken);

                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                var schemaId = schema.TryGetOpenApiSchemaId();
                if (!string.IsNullOrEmpty(schemaId)) context.Document?.Components?.Schemas?.TryAdd(schemaId, schema);

                operation.Parameters ??= [];
                IOpenApiSchema subSchema = underlyingType != null || string.IsNullOrEmpty(schemaId)
                    ? schema
                    : new OpenApiSchemaReference(schemaId, context.Document);

                var openApiParameter = new OpenApiParameter
                {
                    Name = name,
                    In = location.Value,
                    Required = location == ParameterLocation.Path || underlyingType == null,
                    Schema = subSchema
                };

                if (defaultsContainer != null &&
                    TryGetDefaultsValue(defaultsContainer, prop.Name, out var defaultValue))
                {
                    var schemaDefault = GetDefault(defaultValue);
                    if (schemaDefault == null) continue;
                    (subSchema as OpenApiSchema)?.Default = schemaDefault;
                    (subSchema as OpenApiSchemaReference)?.Default = schemaDefault;
                    openApiParameter.Required = false;
                }

                operation.Parameters.Add(openApiParameter);
            }
        }
    }

    private static bool HasGenerateBindAttribute(Type type)
        => type.GetCustomAttributes(inherit: false)
            .Any(a => a.GetType().FullName == GenerateBindAttributeFullName);

    private static (ParameterLocation? location, string? name) GetParameterLocationAndName(PropertyInfo prop)
    {
        if (prop.GetCustomAttribute<FromRouteAttribute>() is { } fromRoute)
            return (ParameterLocation.Path, fromRoute.Name ?? prop.Name);

        if (prop.GetCustomAttribute<FromQueryAttribute>() is { } fromQuery)
            return (ParameterLocation.Query, fromQuery.Name ?? prop.Name);

        if (prop.GetCustomAttribute<FromHeaderAttribute>() is { } fromHeader)
            return (ParameterLocation.Header, fromHeader.Name ?? prop.Name);

        return (null, null); // не обрабатываем body и другие источники
    }

    private static bool TryGetDefaultsValue(Type defaultsContainer, string memberName, out object? value)
    {
        // ищем static field
        var field = defaultsContainer.GetField(memberName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            value = field.GetValue(null);
            return true;
        }

        // ищем static property
        var prop = defaultsContainer.GetProperty(memberName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            value = prop.GetValue(null);
            return true;
        }

        value = null;
        return false;
    }

    private static JsonNode? GetDefault(object? value)
    {
        if (value == null) return null;

        var type = value.GetType();

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
            {
                return JsonValue.Create(value);
            }
        }

        if (type.IsGenericType)
        {
            var typeDefinition = type.GetGenericTypeDefinition();
            if (typeDefinition == typeof(SortCriteria<>))
            {
                var fieldPropertyInfo = type.GetProperty(nameof(SortCriteria<>.Field));
                var fieldValue = fieldPropertyInfo?.GetValue(value)?.ToString()?.ToLowerInvariant();
                var orderPropertyInfo = type.GetProperty(nameof(SortCriteria<>.Order));
                var orderValue = (SortOrderType?)orderPropertyInfo?.GetValue(value);
                if (fieldValue != null && orderValue != null)
                {
                    return orderValue == SortOrderType.Descending
                        ? JsonValue.Create("-" + fieldValue)
                        : JsonValue.Create(fieldValue);
                }

                return null;
            }

            if (typeDefinition == typeof(SortCriteriaList<>))
            {
                var @enum = type.GetGenericArguments()[0];
                var toArray = type.GetMethod("ToArray", Type.EmptyTypes);
                var array = (Array)toArray!.Invoke(value, null)!;
                var itemType = typeof(SortCriteria<>).MakeGenericType(@enum);
                var fieldPropertyInfo = itemType.GetProperty(nameof(SortCriteria<>.Field));
                var orderPropertyInfo = itemType.GetProperty(nameof(SortCriteria<>.Order));
                var varNames = new JsonArray();
                foreach (var item in array)
                {
                    var fieldValue = fieldPropertyInfo?.GetValue(item)?.ToString()?.ToLowerInvariant();
                    var orderValue = (SortOrderType?)orderPropertyInfo?.GetValue(item);
                    if (fieldValue == null || orderValue == null) return null;

                    varNames.Add(orderValue == SortOrderType.Descending
                        ? JsonValue.Create("-" + fieldValue)
                        : JsonValue.Create(fieldValue));
                }

                return varNames;
            }
        }

        var vogenInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IVogen<,>));

        if (vogenInterface == null)
            return null;

        try
        {
            var prop = vogenInterface.GetProperty(nameof(IVogen<,>.Value));
            var primitiveValue = prop?.GetValue(value);

            return primitiveValue is null ? null : JsonValue.Create(primitiveValue);
        }
        catch
        {
            return null;
        }
    }
}