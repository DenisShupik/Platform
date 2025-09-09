using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
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

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var (location, name) = GetParameterLocationAndName(prop);
                if (location == null) continue;


                var schema = await context.GetOrCreateSchemaAsync(prop.PropertyType, null, cancellationToken);
                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                var schemaId = schema.TryGetOpenApiSchemaId();
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = name,
                    In = location.Value,
                    Required = location == ParameterLocation.Path,
                    Schema = underlyingType != null || schemaId == null
                        ? schema
                        : new OpenApiSchemaReference(schemaId, context.Document)
                });
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

    // private static bool IsRequired(PropertyInfo prop)
    // {
    //     return prop.SetMethod?.IsInitOnly == true &&
    //            prop.GetCustomAttributes<RequiredAttribute>().Any();
    // }
}