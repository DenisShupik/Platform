using System;

namespace SharedKernel.Infrastructure.Generator.Attributes;

public enum SortGenerationType
{
    Single = 0,
    Multi = 1
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class AddApplySortAttribute(Type enumType, Type entityType, SortGenerationType sortGenerationType) : Attribute
{
    public Type EnumType { get; } = enumType;
    public Type EntityType { get; } = entityType;
    public SortGenerationType GenerationType { get; } = sortGenerationType;
}