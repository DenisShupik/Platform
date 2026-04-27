using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Vogen;
using static Shared.Application.Helpers.ValidationHelper;

namespace Shared.Presentation.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationLimitMin10Max100 : IPaginationLimit,
    IHasTryFrom<PaginationLimitMin10Max100, int>, IVogen<PaginationLimitMin10Max100, int>
{
    public static int Min => 10;
    public static int Max => 100;

    public static readonly PaginationLimitMin10Max100 Default100 = From(100);

    private static Validation Validate(in int value) => LimitValidation<PaginationLimitMin10Max100>(value);
}