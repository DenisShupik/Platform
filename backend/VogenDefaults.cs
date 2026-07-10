using Vogen;

[assembly:
    VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.ValueObjectsDeriveFromTheInterface | StaticAbstractsGeneration.OmitInterfaceDeclaration,
        conversions: Conversions.SystemTextJson
    )
]