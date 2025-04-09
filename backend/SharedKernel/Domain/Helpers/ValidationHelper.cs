using System.Runtime.CompilerServices;
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
    public static Validation StringValidate(in string value, in int limit) => string.IsNullOrWhiteSpace(value)
        ? Validation.Invalid("Cannot be empty")
        : value.Length > limit
            ? Validation.Invalid($"Cannot exceed {limit} characters")
            : Validation.Ok;
}