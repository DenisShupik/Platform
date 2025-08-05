using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Presentation.Helpers;

public static class OpenApiHelper
{
    private const string UuidPattern = "^(?!00000000-0000-0000-0000-000000000000$)";
    private const string NonEmptyPattern = @"^(?!\s*$).+";

    public static void SetUuidId(OpenApiSchema schema)
    {
        schema.Type = "string";
        schema.Format = "uuid";
        schema.Pattern = UuidPattern;
        schema.Properties = null;
        schema.Required = null;
    }

    public static void SetLongId(OpenApiSchema schema)
    {
        schema.Type = "integer";
        schema.Format = "int64";
        schema.Minimum = 1;
        schema.Properties = null;
        schema.Required = null;
    }

    public static void SetStringLike<T>(OpenApiSchema schema) where T : IHasMinLength, IHasMaxLength
    {
        schema.Type = "string";
        schema.MinLength = T.MinLength;
        schema.MaxLength = T.MaxLength;
        schema.Pattern = NonEmptyPattern;
        schema.Properties = null;
        schema.Required = null;
    }

    public static OpenApiSchema CreateSortEnum(OpenApiSchema schema)
    {
        var enumNames = ((OpenApiArray)schema.Properties["field"].Extensions["x-enum-varnames"])
            .Select(e => ((OpenApiString)e).Value)
            .ToList();

        var newSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = enumNames
                .Select(IOpenApiAny (name) => new OpenApiString(name.ToLowerInvariant()))
                .Concat(enumNames.Select(IOpenApiAny (name) => new OpenApiString("-" + name.ToLowerInvariant())))
                .ToList(),
            Example = null
        };

        var varNames = new OpenApiArray();
        varNames.AddRange(enumNames.Select(name => new OpenApiString($"{name}Asc")));
        varNames.AddRange(enumNames.Select(name => new OpenApiString($"{name}Desc")));
        newSchema.Extensions["x-enum-varnames"] = varNames;

        var descriptions = new OpenApiArray();
        descriptions.AddRange(enumNames.Select(name => new OpenApiString($"Sort by {name} ascending")));
        descriptions.AddRange(enumNames.Select(name => new OpenApiString($"Sort by {name} descending")));
        newSchema.Extensions["x-enum-descriptions"] = descriptions;

        return newSchema;
    }
}