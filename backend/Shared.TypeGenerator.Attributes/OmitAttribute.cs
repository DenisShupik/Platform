namespace Shared.TypeGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OmitAttribute(Type sourceType, PropertyGenerationMode mode, params string[] propertyNames) : Attribute
{
    public Type SourceType { get; } = sourceType;
    public PropertyGenerationMode Mode { get; } = mode;
    public string[] PropertyNames { get; } = propertyNames;
}