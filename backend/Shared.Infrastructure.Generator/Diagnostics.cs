using Microsoft.CodeAnalysis;

namespace Shared.Infrastructure.Generator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor InternalError = new(
        id: "GP0000",
        title: "Source generator internal error",
        messageFormat: "An unexpected exception occurred inside the source generator: {0}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}