using SharedKernel.Domain.Helpers;
using Vogen;

[assembly:
    VogenDefaults(staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon |
                                             StaticAbstractsGeneration.InstanceMethodsAndProperties)]

namespace SharedKernel.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson)]
public readonly partial struct UserId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}