using System.Runtime.CompilerServices;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Domain.Helpers;

public static class ValidationHelper
{
    public static class Constants
    {
        public const string NonEmptyRegexPattern = @"^(?!\s*$).+";
        public const string UuidRegexPattern = "^(?!00000000-0000-0000-0000-000000000000$)";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation UintValidate(in uint value) => Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation LongValidate(in long value) =>
        value > 0 ? Validation.Ok : Validation.Invalid("Must be greater than 0");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation GuidValidate(in Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Cannot be 00000000-0000-0000-0000-000000000000") : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation NonEmptyStringValidate<T>(in string value) where T : INonEmptyString
    {
        if (value.Length < T.MinLength)
            return Validation.Invalid($"Must be at least {T.MinLength} characters long");

        if (value.Length > T.MaxLength)
            return Validation.Invalid($"Cannot exceed {T.MaxLength} characters");

        if (string.IsNullOrWhiteSpace(value)) return Validation.Invalid("Cannot be empty");

        return Validation.Ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation PatternStringValidate<T>(in string value) where T : IRegexString
    {
        if (value.Length < T.MinLength)
            return Validation.Invalid($"Must be at least {T.MinLength} characters long");

        if (value.Length > T.MaxLength)
            return Validation.Invalid($"Cannot exceed {T.MaxLength} characters");

        if (!T.Regex.IsMatch(value)) return Validation.Invalid(T.RegexValidationError);

        return Validation.Ok;
    }
}