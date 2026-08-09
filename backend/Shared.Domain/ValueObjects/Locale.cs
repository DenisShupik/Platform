using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Vogen;

namespace Shared.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Locale
{
    public const string EnglishCode = "en";
    public const string RussianCode = "ru";

    public static Locale English { get; } = From(EnglishCode);
    public static Locale Russian { get; } = From(RussianCode);

    public static IReadOnlyList<string> SupportedCodes { get; } = [EnglishCode, RussianCode];

    private static Validation Validate(in string value) => value is EnglishCode or RussianCode
        ? Validation.Ok
        : Validation.Invalid(ValidationErrorCodec.Encode(ValidationErrorCodes.UnsupportedLocale));
}
