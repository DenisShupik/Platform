using System.Runtime.CompilerServices;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace SharedKernel.Domain.Helpers;

public static class ValidationHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation UintValidate(in uint value) => Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation LongValidate(in long value) =>
        value > 0 ? Validation.Ok : Validation.Invalid("Must be greater than 0");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation GuidValidate(in Guid value) =>
        value == Guid.Empty ? Validation.Invalid($"Cannot be {Guid.Empty}") : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation StringValidate<T>(in string value) where T : IHasMinLength, IHasMaxLength
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Cannot be empty");

        if (value.Length < T.MinLength)
            return Validation.Invalid($"Must be at least {T.MinLength} characters long");

        if (value.Length > T.MaxLength)
            return Validation.Invalid($"Cannot exceed {T.MaxLength} characters");

        return Validation.Ok;
    }
}