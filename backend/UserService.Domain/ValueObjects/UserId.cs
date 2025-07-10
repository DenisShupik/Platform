using ProtoBuf;
using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

[assembly:
    VogenDefaults(staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon |
                                             StaticAbstractsGeneration.InstanceMethodsAndProperties
    )
]

namespace UserService.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson)]
[ProtoContract(Surrogate = typeof(Guid))]
public readonly partial struct UserId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}