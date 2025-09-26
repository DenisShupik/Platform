using Shared.Application.Helpers;
using Shared.Application.Interfaces;
using Vogen;

namespace Shared.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationLimit : IPaginationLimit
{
    public static int Min => 1;
    public static int Max => int.MaxValue;

    private static Validation Validate(in int value) => ValidationHelper.LimitValidation<PaginationLimit>(value);
}