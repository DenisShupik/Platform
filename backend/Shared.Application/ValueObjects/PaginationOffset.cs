using Shared.Application.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationOffset : IHasTryFrom<PaginationOffset, int>
{
    public static readonly PaginationOffset Default = From(0);
    private static Validation Validate(in int value) => ValidationHelper.OffsetValidate(value);
}