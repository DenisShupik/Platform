using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct ThreadTitle : INonEmptyString
{
    public static int MinLength => 3;
    public static int MaxLength => 128;
    private static Validation Validate(in string value) => ValidationHelper.NonEmptyStringValidate<ThreadTitle>(value);
    private static string NormalizeInput(string input) => input.Trim();
}