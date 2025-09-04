using System;

namespace SharedKernel.Infrastructure.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AddApplySortAttribute(Type pagedQueryType, Type entityType) : Attribute
{
    public Type PagedQueryType { get; } = pagedQueryType;
    public Type EntityType { get; } = entityType;
}