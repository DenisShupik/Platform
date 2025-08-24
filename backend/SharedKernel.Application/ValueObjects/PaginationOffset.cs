using SharedKernel.Application.Helpers;
using Vogen;

namespace SharedKernel.Application.ValueObjects;

[ValueObject<int>]
public readonly partial struct PaginationOffset : IVogen<PaginationOffset, int>
{
    private static Validation Validate(in int value) => ValidationHelper.OffsetValidate(value);
}