using System;

namespace Generator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OmitAttribute(Type sourceType, params string[] propertyNames) : Attribute
{
    public Type SourceType { get; } = sourceType;
    public string[] PropertyNames { get; } = propertyNames;
}