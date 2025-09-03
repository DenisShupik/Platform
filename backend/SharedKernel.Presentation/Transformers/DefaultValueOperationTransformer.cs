using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;

namespace SharedKernel.Presentation.Transformers;

public sealed class DefaultValueOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation.Parameters == null) return Task.CompletedTask;

        foreach (var openApiParameter in operation.Parameters)
        {
            if (openApiParameter.Name == null) continue;
            switch (openApiParameter.Schema)
            {
                case OpenApiSchema schema:
                {
                    var defaultValue =
                        TryGetDefaultFromActionParameterInstance(context.Description, openApiParameter.Name);

                    var schemaDefault = GetDefault(defaultValue);
                    if (schemaDefault == null) continue;
                    schema.Default = schemaDefault;
                    (openApiParameter as OpenApiParameter)?.Required = false;
                    break;
                }
                case OpenApiSchemaReference schema:
                {
                    var defaultValue =
                        TryGetDefaultFromActionParameterInstance(context.Description, openApiParameter.Name);

                    var schemaDefault = GetDefault(defaultValue);
                    if (schemaDefault == null) continue;
                    schema.Default = schemaDefault;
                    (openApiParameter as OpenApiParameter)?.Required = false;
                    break;
                }
            }
        }

        return Task.CompletedTask;
    }

    private static object? TryGetDefaultFromActionParameterInstance(ApiDescription apiDescription, string propertyName)
    {
        foreach (var parameterDescription in apiDescription.ParameterDescriptions)
        {
            if (parameterDescription.Name != propertyName) continue;
            if (parameterDescription.ModelMetadata.ContainerType == null) return null;
            var prop = parameterDescription.ModelMetadata.ContainerType.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null) return null;
            try
            {
                var instance = Activator.CreateInstance(parameterDescription.ModelMetadata.ContainerType);
                if (instance == null) return null;

                var value = prop.GetValue(instance);
                if (value != null) return value;
            }
            catch
            {
                // ignore
            }
        }

        return null;
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