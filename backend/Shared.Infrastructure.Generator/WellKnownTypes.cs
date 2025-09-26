using System.Linq;
using Microsoft.CodeAnalysis;

namespace Shared.Infrastructure.Generator;

internal sealed class WellKnownTypes
{
    private readonly Compilation _compilation;
    
    public INamedTypeSymbol? SingleSortPagedQuery { get; }
    public INamedTypeSymbol? MultiSortPagedQuery { get; }
    public INamedTypeSymbol? SortExpressionAttribute { get; }

    public WellKnownTypes(Compilation compilation)
    {
        _compilation = compilation;
        SingleSortPagedQuery =
            compilation.GetTypeByMetadataName("Shared.Application.Abstractions.SingleSortPagedQuery`2");
        MultiSortPagedQuery =
            compilation.GetTypeByMetadataName("Shared.Application.Abstractions.MultiSortPagedQuery`2");
        SortExpressionAttribute =
            compilation.GetTypeByMetadataName("Shared.Infrastructure.Generator.SortExpressionAttribute`1");
    }

    private const string SortTypeName = "SortType";

    public INamedTypeSymbol? FindSortTypeEnum(ITypeSymbol containerType)
    {
        var nested = containerType
            .GetTypeMembers(SortTypeName)
            .FirstOrDefault(t => t.TypeKind == TypeKind.Enum);

        if (nested != null) return nested;

        return _compilation.GetTypeByMetadataName(containerType.ContainingNamespace.ToDisplayString() + "." +
                                                  containerType.Name + SortTypeName);
    }
}