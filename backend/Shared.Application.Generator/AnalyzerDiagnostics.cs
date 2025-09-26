using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Shared.Application.Generator;

internal static class AnalyzerDiagnostics
{
    private const string CategoryNaming = "Naming";

    private static DiagnosticDescriptor CreateNamingRule(
        string id,
        string suffix,
        string interfaceName
    ) =>
        new(
            id,
            $"{suffix} naming",
            "A type implementing '{0}' must have a name ending with '" + suffix + "'",
            CategoryNaming,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            $"Enforces a naming convention: all types implementing {interfaceName} must end with '{suffix}'."
        );

    public static readonly DiagnosticDescriptor QueryNameRule = CreateNamingRule("A001", "Query", "IQuery");
    public static readonly DiagnosticDescriptor CommandNameRule = CreateNamingRule("A002", "Command", "ICommand");

    public static readonly DiagnosticDescriptor QueryHandlerNameRule =
        CreateNamingRule("A003", "QueryHandler", "IQueryHandler");

    public static readonly DiagnosticDescriptor CommandHandlerNameRule =
        CreateNamingRule("A004", "CommandHandler", "ICommandHandler");

    public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =
    [
        QueryNameRule,
        CommandNameRule,
        QueryHandlerNameRule,
        CommandHandlerNameRule
    ];

    public static void Report(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location,
        params object?[]? messageArgs)
    {
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
    }
}