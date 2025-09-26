using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Shared.Infrastructure.Generator;

public sealed class GenerateApplySortData
{
    public sealed class AttributeArguments
    {
        public readonly ITypeSymbol PagedQueryType;
        public readonly ITypeSymbol EntityType;

        public AttributeArguments(ITypeSymbol pagedQueryType, ITypeSymbol entityType)
        {
            PagedQueryType = pagedQueryType;
            EntityType = entityType;
        }
    }
    
    public readonly INamedTypeSymbol ClassSymbol;
    public readonly List<AttributeArguments> Arguments;
    
    public GenerateApplySortData(INamedTypeSymbol classSymbol, List<AttributeArguments> arguments)
    {
        ClassSymbol = classSymbol;
        Arguments = arguments;
    }
}