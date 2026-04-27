using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct UserId : IId, IHasTryFrom<UserId, Guid>
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}