using System;

namespace Generator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IncludeAttribute(Type sourceType, PropertyGenerationMode mode, params string[] propertyNames) : Attribute
{
    public Type SourceType { get; } = sourceType;
    public PropertyGenerationMode Mode { get; } = mode;
    public string[] PropertyNames { get; } = propertyNames;
}