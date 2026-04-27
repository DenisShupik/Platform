using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct Index : IHasTryFrom<Index, int>
{
    public static readonly Index Default = From(0);
    private static Validation Validate(in int value) => ValidationHelper.IndexValidate(value);
}