using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Application.Interfaces;
using Shared.Application.ValueObjects;
using Shared.Domain.Interfaces;
using Shared.Presentation.Extensions;
using static Shared.Domain.Helpers.ValidationHelper.Constants;

namespace Shared.Presentation.Transformers;

public sealed class ValueObjectSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        var vogenInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IVogen<,>));

        if (vogenInterface == null) return Task.CompletedTask;

        Transform(schema, vogenInterface, type);
        if (context.Document == null) throw new OpenApiException("Document cannot be null");
        var schemaId = schema.GetOpenApiSchemaId();
        context.Document.Components?.Schemas?.TryAdd(schemaId, schema);
        context.Document.Workspace?.RegisterComponentForDocument(context.Document, schema, schemaId);
        return Task.CompletedTask;
    }

    private static void Transform(OpenApiSchema schema, Type vogenInterface, Type type)
    {
        var primitive = vogenInterface.GetGenericArguments()[1];
        if (typeof(IId).IsAssignableFrom(type))
        {
            if (primitive == typeof(Guid))
            {
                schema.Type = JsonSchemaType.String;
                schema.Format = "uuid";
                schema.Pattern = UuidRegexPattern;
            }
        }
        else if (typeof(INonEmptyString).IsAssignableFrom(type))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = ReadStaticIntProperty(nameof(INonEmptyString.MinLength), type);
            schema.MaxLength = ReadStaticIntProperty(nameof(INonEmptyString.MaxLength), type);
            schema.Pattern = NonEmptyRegexPattern;
        }
        else if (typeof(IRegexString).IsAssignableFrom(type))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = ReadStaticIntProperty(nameof(IRegexString.MinLength), type);
            schema.MaxLength = ReadStaticIntProperty(nameof(IRegexString.MaxLength), type);
            schema.Pattern = ReadStaticStringProperty(nameof(IRegexString.Regex), type);
        }
        else if (typeof(IPaginationLimit).IsAssignableFrom(type))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Minimum = ReadStaticIntProperty(nameof(IPaginationLimit.Min), type).ToString();
            schema.Maximum = ReadStaticIntProperty(nameof(IPaginationLimit.Max), type).ToString();
        }
        else if (typeof(PaginationOffset).IsAssignableFrom(type))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Minimum = 0.ToString();
            schema.Maximum = int.MaxValue.ToString();
            schema.Default = 0;
        }
        else if (primitive == typeof(ulong))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "uint64";
            schema.Pattern = null;
        }
        else
        {
            throw new OpenApiException($"Type {type.FullName} must be properly mapped to OpenApi");
        }
    }

    private const BindingFlags StaticPublicFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    private static int ReadStaticIntProperty(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, StaticPublicFlags)
                           ?? throw new OpenApiException(
                               $"Type {type.FullName} must expose public static int '{propertyName}' property");

        var value = propertyInfo.GetValue(null)
                    ?? throw new OpenApiException(
                        $"Static property '{propertyName}' on {type.FullName} returned null or has no getter.");

        return Convert.ToInt32(value);
    }

    private static string? ReadStaticStringProperty(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, StaticPublicFlags)
                           ?? throw new OpenApiException(
                               $"Type {type.FullName} must expose public static string '{propertyName}' property");

        var value = propertyInfo.GetValue(null)
                    ?? throw new OpenApiException(
                        $"Static property '{propertyName}' on {type.FullName} returned null or has no getter.");

        return Convert.ToString(value);
    }
}