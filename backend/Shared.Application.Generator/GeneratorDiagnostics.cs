using Microsoft.CodeAnalysis;

namespace Shared.Application.Generator;

internal static class GeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor InternalError = new(
        id: "G000",
        title: "Source generator internal error",
        messageFormat: "An unexpected exception occurred inside the source generator: {0}",
        category: "Generator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}