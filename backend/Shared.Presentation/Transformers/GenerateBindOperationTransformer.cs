using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Presentation.Binding;
using Shared.Presentation.Extensions;

namespace Shared.Presentation.Transformers;

public sealed class GenerateBindOperationTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var requestMetadata in context.Description.ActionDescriptor.EndpointMetadata
                     .OfType<GeneratedRequestBindingMetadata>())
        {
            foreach (var parameter in requestMetadata.Parameters)
            {
                var schema = await context.GetOrCreateSchemaAsync(
                    parameter.ParameterType,
                    null,
                    cancellationToken);
                var parameterSchema = schema.CreateShallowCopy();
                var required = parameter.Source == GeneratedRequestParameterSource.Path || !parameter.IsNullable;
                if (parameter.HasDefault)
                {
                    parameterSchema = WithDefault(parameterSchema, CreateDefault(parameter.DefaultValue));
                    required = false;
                }

                AddOrReplaceParameter(operation, new OpenApiParameter
                {
                    Name = parameter.Name,
                    In = MapToOpenApiParameterLocation(parameter.Source),
                    Required = required,
                    Schema = parameterSchema
                });
            }
        }
    }

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

    private static IOpenApiSchema WithDefault(IOpenApiSchema schema, JsonNode value)
    {
        switch (schema)
        {
            case OpenApiSchema concreteSchema when OpenApiSchemaReferenceId.Get(concreteSchema) is not null:
                // The document transformer will promote this annotated schema to a
                // component reference. Keep operation-local keywords on a wrapper so
                // they are not discarded when that promotion happens.
                return new OpenApiSchema
                {
                    AllOf = [concreteSchema],
                    Default = value
                };
            case OpenApiSchema concreteSchema:
                concreteSchema.Default = value;
                return concreteSchema;
            case OpenApiSchemaReference referenceSchema:
                // Microsoft.OpenApi does not serialize JSON Schema keywords added to a
                // reference instance. Keep the reusable schema and put the default on a
                // local schema that composes it instead.
                return new OpenApiSchema
                {
                    AllOf = [referenceSchema],
                    Default = value
                };
            default:
                throw new OpenApiException($"Schema type {schema.GetType().FullName} cannot define a default");
        }
    }

    private static ParameterLocation MapToOpenApiParameterLocation(GeneratedRequestParameterSource source) =>
        source switch
        {
            GeneratedRequestParameterSource.Path => ParameterLocation.Path,
            GeneratedRequestParameterSource.Query => ParameterLocation.Query,
            _ => throw new OpenApiException($"Unsupported generated request parameter source {source}")
        };
}
