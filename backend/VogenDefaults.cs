using Vogen;

[assembly:
    VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.ValueObjectsDeriveFromTheInterface,
        conversions: Conversions.SystemTextJson
    )
]