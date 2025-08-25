using System;

namespace SharedKernel.Infrastructure.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AddApplySortAttribute(Type enumType, Type entityType) : Attribute
{
    public Type EnumType { get; } = enumType;
    public Type EntityType { get; } = entityType;
}