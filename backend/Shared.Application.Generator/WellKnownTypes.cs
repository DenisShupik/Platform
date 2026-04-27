using Microsoft.CodeAnalysis;

namespace Shared.Application.Generator;

internal sealed class WellKnownTypes
{
    public INamedTypeSymbol? Query { get; }
    public INamedTypeSymbol? Command { get; }
    public INamedTypeSymbol? QueryHandler { get; }
    public INamedTypeSymbol? CommandHandler { get; }

    public WellKnownTypes(Compilation compilation)
    {
        Query = compilation.GetTypeByMetadataName("Shared.Application.Interfaces.IQuery`1");
        Command = compilation.GetTypeByMetadataName("Shared.Application.Interfaces.ICommand`1");
        QueryHandler = compilation.GetTypeByMetadataName("Shared.Application.Interfaces.IQueryHandler`2");
        CommandHandler = compilation.GetTypeByMetadataName("Shared.Application.Interfaces.ICommandHandler`2");
    }
}