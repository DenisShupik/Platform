namespace Shared.Presentation.Generator.Attributes;

/// <summary>
/// Generates JSON converters for every supported <c>Result&lt;TValue, TError...&gt;</c> arity.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateResultJsonConvertersAttribute : Attribute;
