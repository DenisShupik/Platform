using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Shared.Generator;
using Shared.Presentation.Generator.Enums;
using static Shared.Presentation.Generator.Diagnostics;

namespace Shared.Presentation.Generator;

public sealed partial class Generator
{
    public sealed class ClassContext
    {
        private const string DefaultsClassName = "Defaults";

        private readonly INamedTypeSymbol _classSymbol;
        private readonly INamedTypeSymbol? _defaults;
        private readonly WellKnownTypes _wellKnownTypes;

        public readonly List<Diagnostic> Diagnostics;
        public readonly IPropertySymbol[] Properties;

        public ClassContext(INamedTypeSymbol classSymbol, WellKnownTypes wellKnownTypes)
        {
            _classSymbol = classSymbol;
            _defaults = classSymbol.GetTypeMembers(DefaultsClassName).FirstOrDefault(t => t.TypeKind == TypeKind.Class);
            _wellKnownTypes = wellKnownTypes;
            Diagnostics = [];
            Properties = classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public).ToArray();
            if (_defaults == null) return;
            {
                var propNames = new HashSet<string>(Properties.Select(p => p.Name));
                foreach (var member in _defaults.GetMembers().Where(m => !m.IsImplicitlyDeclared))
                {
                    if (member is not (IFieldSymbol or IPropertySymbol)) continue;
                    if (propNames.Contains(member.Name)) continue;

                    var loc = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation()
                              ?? _defaults.Locations.FirstOrDefault() ?? Location.None;
                    Diagnostics.Add(Diagnostic.Create(DefaultsMemberNotFound, loc, member.Name));
                }
            }
        }

        public string? TryGetDefaultField(IPropertySymbol property)
        {
            var defaultFiled = _defaults?.GetMembers(property.Name)
                .OfType<IFieldSymbol>()
                .FirstOrDefault();

            if (defaultFiled == null) return null;

            if (defaultFiled is { IsStatic: true, IsReadOnly: true } &&
                SymbolEqualityComparer.Default.Equals(defaultFiled.Type, property.Type) &&
                defaultFiled.HasConstantValue || defaultFiled.HasInitializer())
            {
                return $"{_classSymbol.Name}.{DefaultsClassName}.{defaultFiled.Name}";
            }

            return null;
        }

        public bool TryGetBindingKind(
            IPropertySymbol property,
            Location? loc,
            [NotNullWhen(true)] out ParameterBinding? bindingKind
        )
        {
            ParameterBinding? found = null;

            foreach (var attribute in property.GetAttributes().Select(attribute => attribute.AttributeClass)
                         .OfType<INamedTypeSymbol>())
            {
                ParameterBinding? candidate;
                if (SymbolEqualityComparer.Default.Equals(attribute, _wellKnownTypes.FromRouteAttribute))
                    candidate = ParameterBinding.FromRoute;
                else if (SymbolEqualityComparer.Default.Equals(attribute, _wellKnownTypes.FromQueryAttribute))
                    candidate = ParameterBinding.FromQuery;
                else if (SymbolEqualityComparer.Default.Equals(attribute, _wellKnownTypes.FromBodyAttribute))
                    candidate = ParameterBinding.FromBody;
                else
                    continue;

                if (found is not null)
                {
                    Diagnostics.Add(Diagnostic.Create(MultipleBindingAttribute, loc, property.Name));
                    bindingKind = null;
                    return false;
                }

                found = candidate;
            }

            if (found is null)
            {
                Diagnostics.Add(Diagnostic.Create(MissingBindingAttribute, loc, property.Name));
                bindingKind = null;
                return false;
            }

            bindingKind = found;
            return true;
        }
    }
}