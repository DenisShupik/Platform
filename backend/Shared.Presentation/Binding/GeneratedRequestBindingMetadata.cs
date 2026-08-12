namespace Shared.Presentation.Binding;

public enum GeneratedRequestParameterSource : byte
{
    Path,
    Query
}

public sealed record GeneratedRequestParameterMetadata(
    Type ParameterType,
    GeneratedRequestParameterSource Source,
    string Name,
    bool IsNullable,
    bool HasDefault,
    object? DefaultValue);

public sealed record GeneratedRequestBindingMetadata(
    IReadOnlyList<GeneratedRequestParameterMetadata> Parameters);
