using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddPaginationLimitSchemaFilter : ISchemaFilter
{
    private const BindingFlags StaticPublicFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Reference is not null ||
            !typeof(IPaginationLimit).IsAssignableFrom(context.Type))
            return;

        var (min, max, @default) = ReadPaginationProperties(context.Type);

        schema.Type = "integer";
        schema.Minimum = min;
        schema.Maximum = max;
        schema.Default = new OpenApiInteger(@default);
        schema.Properties = null;
        schema.Required = null;
    }

    private static (int Min, int Max, int Default) ReadPaginationProperties(Type type)
    {
        var min = ReadStaticProperty(nameof(IPaginationLimit.Min), type);
        var max = ReadStaticProperty(nameof(IPaginationLimit.Max), type);
        var @default = ReadStaticProperty(nameof(IPaginationLimit.Default), type);

        return (min, max, @default);
    }

    private static int ReadStaticProperty(string propertyName, Type type)
    {
        var propertyInfo = type.GetProperty(propertyName, StaticPublicFlags)
                           ?? throw new InvalidOperationException(
                               $"Type {type.FullName} must expose public static '{propertyName}' property required by {nameof(IPaginationLimit)}.");

        var value = propertyInfo.GetMethod?.Invoke(null, [])
                    ?? throw new InvalidOperationException(
                        $"Static property '{propertyName}' on {type.FullName} returned null or has no getter.");

        return Convert.ToInt32(value);
    }
}