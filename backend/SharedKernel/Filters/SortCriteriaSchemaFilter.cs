using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Sorting;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Filters;

public sealed class SortParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo?.ParameterType.IsGenericType != true ||
            context.ParameterInfo.ParameterType.GetGenericTypeDefinition() != typeof(SortCriteria<>)) return;

        var enumType = context.ParameterInfo.ParameterType.GenericTypeArguments[0];
        var enumNames = Enum.GetNames(enumType);

        parameter.Schema.Type = "string";
        parameter.Description = $"Field to sort by with optional '-' prefix for descending order for {enumType.Name}.";
        parameter.Example = new OpenApiString(enumNames.FirstOrDefault());

        parameter.Schema.Enum = enumNames
            .Select(IOpenApiAny (name) => new OpenApiString(name))
            .Concat(enumNames.Select(name => new OpenApiString($"-{name}")))
            .ToList();
    }
}