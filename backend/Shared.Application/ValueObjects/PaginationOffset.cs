using Shared.Application.Helpers;
using Vogen;

namespace Shared.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationOffset
{
    public static readonly PaginationOffset Default = From(0);
    private static Validation Validate(in int value) => ValidationHelper.OffsetValidate(value);
}