using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddPaginationLimitInlineParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        Type? memberType = null;

        if (context.ParameterInfo is { } pInfo)
            memberType = pInfo.ParameterType;
        else if (context.PropertyInfo is { } prInfo)
            memberType = prInfo.PropertyType;

        if (memberType == null) return;

        memberType = Nullable.GetUnderlyingType(memberType) ?? memberType;

        if (!typeof(IPaginationLimit).IsAssignableFrom(memberType)) return;
        if (memberType.IsAbstract || memberType.ContainsGenericParameters) return;

        var hasMin = TryGetStaticInt(memberType, nameof(IPaginationLimit.Min), out var minValue);
        var hasMax = TryGetStaticInt(memberType, nameof(IPaginationLimit.Max), out var maxValue);
        var hasDefault = TryGetStaticInt(memberType, nameof(IPaginationLimit.Default), out var defaultValue);

        var inline = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Nullable = parameter.Schema?.Nullable ?? false
        };

        if (hasMin) inline.Minimum = Convert.ToInt32(minValue);
        if (hasMax) inline.Maximum = Convert.ToInt32(maxValue);
        if (hasDefault) inline.Default = new OpenApiInteger(defaultValue);

        parameter.Schema = inline;
        parameter.Content = null;

        try
        {
            var repo = context.SchemaRepository;
            var key = repo?.Schemas.Keys
                .FirstOrDefault(k => string.Equals(k, memberType.Name, StringComparison.OrdinalIgnoreCase)
                                     || k.EndsWith("." + memberType.Name, StringComparison.OrdinalIgnoreCase));
            if (key != null)
                repo?.Schemas.Remove(key);
        }
        catch
        {
            // ignore
        }
    }

    private static bool TryGetStaticInt(Type t, string name, out int value)
    {
        value = 0;

        var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (prop != null && prop.PropertyType == typeof(int) && prop.GetMethod != null)
        {
            var raw = prop.GetValue(null);
            if (raw != null)
            {
                value = Convert.ToInt32(raw);
                return true;
            }
        }

        var field = t.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field == null || field.FieldType != typeof(int)) return false;
        {
            var raw = field.GetValue(null);
            if (raw == null) return false;
            value = Convert.ToInt32(raw);
            return true;
        }
    }
}