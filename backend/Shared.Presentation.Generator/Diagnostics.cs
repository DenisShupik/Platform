using Microsoft.CodeAnalysis;

namespace Shared.Presentation.Generator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor InternalError = new(
        id: "GP0000",
        title: "Source generator internal error",
        messageFormat: "An unexpected exception occurred inside the source generator: {0}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingBindingAttribute = new(
        id: "GP0001",
        title: "Property must have binding attribute",
        messageFormat: "Property '{0}' must have one of [FromRoute], [FromQuery], [FromBody]",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleBindingAttribute = new(
        id: "GP0002",
        title: "Property must not have multiple binding attributes",
        messageFormat:
        "Property '{0}' must have exactly one binding attribute among [FromRoute], [FromQuery], [FromBody]",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NullableWithDefaultValue = new(
        id: "GP0003",
        title: "Nullable parameter must not define a default value",
        messageFormat:
        "Parameter '{0}' is nullable and must not specify a default value",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FromRouteCannotBeNullable = new(
        id: "GP0004",
        title: "Property marked with [FromRoute] must be non-nullable",
        messageFormat:
        "Property '{0}' is marked with [FromRoute] and must be non-nullable",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FromRouteCannotHaveDefaultValue = new(
        id: "GP0005",
        title: "Member marked with [FromRoute] must not define a default value",
        messageFormat:
        "Member '{0}' is marked with [FromRoute] and must not specify a default value",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DefaultsMemberNotFound = new(
        id: "GP0006",
        title: "Defaults contains member with no matching property",
        messageFormat:
        "Defaults contains member '{0}' but no matching public property '{0}' exists in the enclosing [GenerateBind] type",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InitializerNotAllowed = new(
        id: "GP0007",
        title: "Initializer not allowed in [GenerateBind] class",
        messageFormat:
        "Property '{0}' must not have an initializer in a type annotated with [GenerateBind]. Move default values to the nested Defaults class.",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyMustBeRequired = new(
        id: "GP0008",
        title: "Property must be declared 'required'",
        messageFormat: "Property '{0}' must be declared with the 'required' modifier in a [GenerateBind] type",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyMustHaveGetInit = new(
        id: "GP0009",
        title: "Property must have 'get; init;' accessors",
        messageFormat: "Property '{0}' must declare accessors 'get; init;' (auto-property) in a [GenerateBind] type",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleFromBodyNotAllowed = new(
        id: "GP0010",
        title: "Only one [FromBody] is allowed",
        messageFormat: "Only one property may be annotated with [FromBody] in a [GenerateBind] type",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FromBodyMustBeNamedBody = new(
        id: "GP0011",
        title: "[FromBody] property must be named 'Body'",
        messageFormat: "Property '{0}' is annotated with [FromBody] but must be named 'Body' in a [GenerateBind] type",
        category: "GenerateBind",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}