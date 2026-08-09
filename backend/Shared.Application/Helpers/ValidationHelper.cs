using System.Runtime.CompilerServices;
using Shared.Application.Interfaces;
using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Vogen;

namespace Shared.Application.Helpers;

public static class ValidationHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation OffsetValidate(in int value) =>
        value < 0
            ? Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.MustBeNonNegative))
            : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation LimitValidation<T>(in int value) where T : IPaginationLimit
    {
        if (value < T.Min || value > T.Max)
            return Validation.Invalid(ValidationErrorCodec.Encode(
                ValidationErrorCodes.MustBeWithinInclusiveRange,
                T.Min,
                T.Max));

        return Validation.Ok;
    }
}
