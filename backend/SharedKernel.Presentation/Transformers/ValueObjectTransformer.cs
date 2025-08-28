using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Domain.Interfaces;
using static SharedKernel.Domain.Helpers.ValidationHelper.Constants;

namespace SharedKernel.Presentation.Transformers;

public sealed class ValueObjectTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        var underlyingType = Nullable.GetUnderlyingType(type);
        Type? nullableType = null;
        if (underlyingType != null)
        {
            nullableType = type;
            type = underlyingType;
        }

        var vogenInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IVogen<,>));

        if (vogenInterface == null)
            return;

        if (nullableType != null)
        {
            if (!(context.Document?.Components?.Schemas?.TryGetValue(type.Name, out _) ?? false))
            {
                var newSchema = await context.GetOrCreateSchemaAsync(type, null, cancellationToken);
                Transform(newSchema, vogenInterface, type);
                context.Document?.Components?.Schemas?.Add(type.Name, newSchema);
            }

            schema.Type = null;
            schema.Properties = null;
            schema.Required = null;
            schema.AnyOf =
                new List<IOpenApiSchema>
                {
                    new OpenApiSchemaReference(type.Name, context.Document),
                    new OpenApiSchema { Type = JsonSchemaType.Null }
                };
            schema.Metadata?.Clear();
        }
        else
        {
            Transform(schema, vogenInterface, type);
        }
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
            schema.MinLength = ReadStaticIntProperty(nameof(IPaginationLimit.Min), type);
            schema.MaxLength = ReadStaticIntProperty(nameof(IPaginationLimit.Max), type);
            schema.Default = ReadStaticIntProperty(nameof(IPaginationLimit.Default), type);
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
            schema.Minimum = 0.ToString();
            schema.Maximum = ulong.MaxValue.ToString();
        }
        else
        {
            throw new InvalidOperationException($"Type {type.FullName} must be properly mapped to OpenApi");
        }
    }

    private const BindingFlags StaticPublicFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    private static int ReadStaticIntProperty(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, StaticPublicFlags)
                           ?? throw new InvalidOperationException(
                               $"Type {type.FullName} must expose public static int '{propertyName}' property");

        var value = propertyInfo.GetValue(null)
                    ?? throw new InvalidOperationException(
                        $"Static property '{propertyName}' on {type.FullName} returned null or has no getter.");

        return Convert.ToInt32(value);
    }

    private static string? ReadStaticStringProperty(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, StaticPublicFlags)
                           ?? throw new InvalidOperationException(
                               $"Type {type.FullName} must expose public static string '{propertyName}' property");

        var value = propertyInfo.GetValue(null)
                    ?? throw new InvalidOperationException(
                        $"Static property '{propertyName}' on {type.FullName} returned null or has no getter.");

        return Convert.ToString(value);
    }
}