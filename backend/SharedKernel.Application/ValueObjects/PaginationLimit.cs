using SharedKernel.Application.Helpers;
using SharedKernel.Application.Interfaces;
using Vogen;

namespace SharedKernel.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationLimit : IPaginationLimit
{
    public static int Min => 1;
    public static int Max => int.MaxValue;

    private static Validation Validate(in int value) => ValidationHelper.LimitValidation<PaginationLimit>(value);
}