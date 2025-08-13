using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace UserService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct UserId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}