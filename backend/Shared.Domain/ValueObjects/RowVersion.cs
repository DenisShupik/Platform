using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Domain.ValueObjects;

[ValueObject<uint>]
public readonly partial struct RowVersion : IHasTryFrom<RowVersion, uint>
{
    private static Validation Validate(in uint value) => ValidationHelper.UintValidate(value);
}
