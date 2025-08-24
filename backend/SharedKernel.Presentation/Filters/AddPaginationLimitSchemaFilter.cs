using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddPaginationLimitSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == null) return;

        if (schema.Reference != null) return;

        if (!typeof(IPaginationLimit).IsAssignableFrom(context.Type)) return;

        schema.Type = "integer";
        schema.Minimum = Read(nameof(IPaginationLimit.Min), context.Type);
        schema.Maximum = Read(nameof(IPaginationLimit.Max), context.Type);
        schema.Default = new OpenApiInteger(Read(nameof(IPaginationLimit.Default), context.Type));
        schema.Properties = null;
        schema.Required = null;
    }

    private static int Read(string name, Type type)
    {
        var propertyInfo = type.GetProperty(
            name,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (propertyInfo?.GetMethod == null)
            throw new InvalidOperationException(
                $"Type {type.FullName} must expose public static '{name}' property required by {nameof(IPaginationLimit)}.");

        var value = propertyInfo.GetMethod.Invoke(null, [])
                    ?? throw new InvalidOperationException(
                        $"Static property '{name}' on {type.FullName} returned null.");

        return Convert.ToInt32(value);
    }
}