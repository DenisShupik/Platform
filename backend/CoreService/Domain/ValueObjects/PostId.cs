using SharedKernel.Domain;
using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<long>(conversions: Conversions.SystemTextJson)]
public readonly partial struct PostId : IId
{
    private static Validation Validate(in long value) => ValidationHelper.LongValidate(value);
}