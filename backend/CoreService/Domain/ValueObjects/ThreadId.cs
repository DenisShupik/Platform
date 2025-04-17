using SharedKernel.Domain;
using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson)]
public readonly partial struct ThreadId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}