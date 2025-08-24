using SharedKernel.Application.Interfaces;
using Vogen;
using static SharedKernel.Application.Helpers.ValidationHelper;

namespace SharedKernel.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationLimitMin10Max100Default100 : IPaginationLimit,
    IVogen<PaginationLimitMin10Max100Default100, int>
{
    public static int Min => 10;
    public static int Max => 100;
    public static int Default => 100;

    private static Validation Validate(in int value) => LimitValidation<PaginationLimitMin10Max100Default100>(value);
}