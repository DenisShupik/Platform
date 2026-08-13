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

    public static readonly DiagnosticDescriptor InvalidEndpointSpecification = new(
        id: "GP0100",
        title: "Invalid generated endpoint specification",
        messageFormat: "Endpoint '{0}' is invalid: {1}",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointPropertyCannotBeMapped = new(
        id: "GP0101",
        title: "Application request property cannot be mapped",
        messageFormat: "Endpoint '{0}' cannot map application property '{1}' from request '{2}'",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointDocumentationMissing = new(
        id: "GP0102",
        title: "Endpoint documentation is missing",
        messageFormat: "Documentation operation '{0}' was not found in Documentation/Api.en.xml",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointDocumentationInvalid = new(
        id: "GP0103",
        title: "Endpoint documentation is invalid",
        messageFormat: "Cannot read endpoint documentation: {0}",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointDocumentationDuplicate = new(
        id: "GP0104",
        title: "Endpoint documentation key is duplicated",
        messageFormat: "Documentation operation key '{0}' is declared more than once",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointDocumentationUnused = new(
        id: "GP0105",
        title: "Endpoint documentation is unused",
        messageFormat: "Documentation operation '{0}' does not have a generated endpoint",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RouteParameterNotBound = new(
        id: "GP0106",
        title: "Route parameter is not bound by the request",
        messageFormat:
        "Endpoint '{0}' route parameter '{1}' does not have a matching [FromRoute] property on request '{2}'",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FromRoutePropertyNotInRoute = new(
        id: "GP0107",
        title: "Request route property is missing from the route",
        messageFormat:
        "Endpoint '{0}' [FromRoute] property '{1}' does not have a matching parameter in the complete route pattern",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CreatedAtTargetCannotBeInferred = new(
        id: "GP0108",
        title: "CreatedAt target cannot be inferred",
        messageFormat:
        "Endpoint '{0}' cannot infer a unique GET endpoint for created value '{1}': {2}",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidCreatedAtTarget = new(
        id: "GP0109",
        title: "CreatedAt target is invalid",
        messageFormat:
        "Endpoint '{0}' CreatedAt request '{1}' must be a registered GET endpoint accepting the created value '{2}' as its only [FromRoute] property",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateEndpointName = new(
        id: "GP0110",
        title: "Endpoint name must be globally unique",
        messageFormat: "Generated endpoint name '{0}' is used by more than one endpoint",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor HandlerKindDoesNotMatchHttpMethod = new(
        id: "GP0111",
        title: "Handler kind does not match the HTTP method",
        messageFormat: "Endpoint '{0}' mapped with {1} must use a {2} handler, but '{3}' is a {4} handler",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointNamingConventionMismatch = new(
        id: "GP0112",
        title: "Endpoint types do not follow the same naming convention",
        messageFormat:
        "HTTP request '{0}', application request '{1}', and handler '{2}' should use the same operation stem",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GroupRouteStartsWithSlash = new(
        id: "GP0113",
        title: "Route-group child pattern should not start with a slash",
        messageFormat: "Endpoint '{0}' is mapped on a route group, so its pattern should not start with '/'",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OptionalRouteParameterIsNotSupported = new(
        id: "GP0114",
        title: "Optional route parameter cannot bind to a generated request",
        messageFormat:
        "Endpoint '{0}' route parameter '{1}' is optional, but [FromRoute] property '{2}' is required by generated binding",
        category: "GenerateEndpoint",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
