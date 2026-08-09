using System.Runtime.CompilerServices;
using Shared.Domain.Errors;
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
    public static Validation IndexValidate(in int value) =>
        value < 0
            ? Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.MustBeNonNegative))
            : Validation.Ok;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation CountValidate(in int value) =>
        value < 0
            ? Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.MustBeNonNegative))
            : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation GuidValidate(in Guid value) =>
        value == Guid.Empty
            ? Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.InvalidIdentifier))
            : Validation.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation NonEmptyStringValidate<T>(in string value) where T : INonEmptyString
    {
        if (value.Length < T.MinLength)
            return Validation.Invalid(ValidationErrorCodec.Encode(
                ValidationErrorCodes.StringIsShorterThanMinimumLength,
                T.MinLength));

        if (value.Length > T.MaxLength)
            return Validation.Invalid(ValidationErrorCodec.Encode(
                ValidationErrorCodes.StringExceedsMaximumLength,
                T.MaxLength));

        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.MustNotBeEmpty));

        return Validation.Ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation PatternStringValidate<T>(in string value) where T : IRegexString
    {
        if (value.Length < T.MinLength)
            return Validation.Invalid(ValidationErrorCodec.Encode(
                ValidationErrorCodes.StringIsShorterThanMinimumLength,
                T.MinLength));

        if (value.Length > T.MaxLength)
            return Validation.Invalid(ValidationErrorCodec.Encode(
                ValidationErrorCodes.StringExceedsMaximumLength,
                T.MaxLength));

        if (!T.Regex.IsMatch(value)) return Validation.Invalid(T.RegexValidationError);

        return Validation.Ok;
    }
}
