using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Shared.Application.Generator.AnalyzerDiagnostics;

namespace Shared.Application.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        AnalyzerDiagnostics.SupportedDiagnostics;

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var wellKnown = new WellKnownTypes(compilationStartContext.Compilation);
            context.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, wellKnown),
                SymbolKind.NamedType);
        });
    }

    private enum Kind : byte
    {
        Query,
        Command,
        QueryHandler,
        CommandHandler
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, WellKnownTypes wellKnown)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        Kind? kind = null;
        foreach (var symbol in namedType.AllInterfaces.Select(i => i.OriginalDefinition))
        {
            if (SymbolEqualityComparer.Default.Equals(symbol, wellKnown.Query))
            {
                kind = Kind.Query;
                break;
            }

            if (SymbolEqualityComparer.Default.Equals(symbol, wellKnown.Command))
            {
                kind = Kind.Command;
                break;
            }

            if (SymbolEqualityComparer.Default.Equals(symbol, wellKnown.QueryHandler))
            {
                kind = Kind.QueryHandler;
                break;
            }

            if (SymbolEqualityComparer.Default.Equals(symbol, wellKnown.CommandHandler))
            {
                kind = Kind.CommandHandler;
                break;
            }
        }

        var name = namedType.Name;
        var location = namedType.Locations.FirstOrDefault() ?? namedType.Locations[0];

        switch (kind)
        {
            case Kind.Query:
            {
                if (!name.EndsWith("Query")) context.Report(QueryNameRule, location, name);
                break;
            }
            case Kind.Command:
            {
                if (!name.EndsWith("Command")) context.Report(CommandNameRule, location, name);
                break;
            }
            case Kind.QueryHandler:
            {
                if (!name.EndsWith("QueryHandler")) context.Report(QueryHandlerNameRule, location, name);
                break;
            }
            case Kind.CommandHandler:
            {
                if (!name.EndsWith("CommandHandler")) context.Report(CommandHandlerNameRule, location, name);
                break;
            }
        }
    }
}