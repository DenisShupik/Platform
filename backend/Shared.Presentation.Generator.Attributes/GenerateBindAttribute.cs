namespace Shared.Presentation.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateBindAttribute(AuthorizeMode authorizeMode) : Attribute;
