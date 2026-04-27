using System.Runtime.CompilerServices;
using Shared.Application.Interfaces;
using Vogen;

namespace Shared.Application.Helpers;

public static class ValidationHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation OffsetValidate(in int value) =>
        value < 0 ? Validation.Invalid("Must be greater than 0") : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation LimitValidation<T>(in int value) where T : IPaginationLimit
    {
        if (value < T.Min || value > T.Max)
            return Validation.Invalid($"Value must be between {T.Min} and {T.Max} (inclusive)");

        return Validation.Ok;
    }
}