using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Shared.Presentation.Generator.Enums;

namespace Shared.Presentation.Generator;

public sealed class WellKnownTypes
{
    public INamedTypeSymbol? FromRouteAttribute { get; }
    public INamedTypeSymbol? FromBodyAttribute { get; }
    public INamedTypeSymbol? FromQueryAttribute { get; }
    public INamedTypeSymbol? IVogenInterface { get; }
    public INamedTypeSymbol? IValueTypeWithTryParseExtended { get; }
    public INamedTypeSymbol? IReferenceTypeWithTryParseExtended { get; }

    public WellKnownTypes(Compilation compilation)
    {
        FromRouteAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromRouteAttribute");
        FromBodyAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
        FromQueryAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromQueryAttribute");
        IValueTypeWithTryParseExtended =
            compilation.GetTypeByMetadataName("Shared.Domain.Interfaces.IValueTypeWithTryParseExtended`1");
        IReferenceTypeWithTryParseExtended =
            compilation.GetTypeByMetadataName("Shared.Domain.Interfaces.IReferenceTypeWithTryParseExtended`1");
        IVogenInterface = compilation.GetTypeByMetadataName("IVogen`2");
    }

    private bool TryGetIVogenArguments(
        ITypeSymbol type,
        [NotNullWhen(true)] out ITypeSymbol? arg0,
        [NotNullWhen(true)] out ITypeSymbol? arg1)
    {
        arg0 = null;
        arg1 = null;

        var genericSymbol = IVogenInterface;

        if (genericSymbol is null) return false;

        var matchingInterface = type.AllInterfaces
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericSymbol));

        if (matchingInterface == null || matchingInterface.TypeArguments.Length != 2)
            return false;

        arg0 = matchingInterface.TypeArguments[0];
        arg1 = matchingInterface.TypeArguments[1];
        return true;
    }

    private bool TryGetIValueTypeWithTryParseExtendedArguments(
        ITypeSymbol type,
        [NotNullWhen(true)] out ITypeSymbol? arg0
    )
    {
        arg0 = null;

        var genericSymbol = IValueTypeWithTryParseExtended;

        if (genericSymbol is null) return false;

        var matchingInterface = type.AllInterfaces
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericSymbol));

        if (matchingInterface == null || matchingInterface.TypeArguments.Length != 1)
            return false;

        arg0 = matchingInterface.TypeArguments[0];

        return true;
    }

    private bool TryGetIReferenceTypeWithTryParseExtendedArguments(
        ITypeSymbol type,
        [NotNullWhen(true)] out ITypeSymbol? arg0
    )
    {
        arg0 = null;

        var genericSymbol =
            IReferenceTypeWithTryParseExtended;

        if (genericSymbol is null) return false;

        var matchingInterface = type.AllInterfaces
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericSymbol));

        if (matchingInterface == null || matchingInterface.TypeArguments.Length != 1)
            return false;

        arg0 = matchingInterface.TypeArguments[0];

        return true;
    }

    public ParameterType GetParameterKind(ITypeSymbol type, out ITypeSymbol? arg0, out ITypeSymbol? arg1)
    {
        {
            if (TryGetIVogenArguments(type, out var t0, out var t1))
            {
                arg0 = t0;
                arg1 = t1;
                return ParameterType.ValueObject;
            }
        }
        {
            if (TryGetIValueTypeWithTryParseExtendedArguments(type, out var t0))
            {
                arg0 = t0;
                arg1 = null;
                return ParameterType.ValueType;
            }
        }
        {
            if (TryGetIReferenceTypeWithTryParseExtendedArguments(type, out var t0))
            {
                arg0 = t0;
                arg1 = null;
                return ParameterType.ReferenceType;
            }
        }
        if (type.TypeKind == TypeKind.Enum)
        {
            arg0 = null;
            arg1 = null;
            return ParameterType.Enum;
        }

        arg0 = null;
        arg1 = null;
        return ParameterType.Primitive;
    }
}