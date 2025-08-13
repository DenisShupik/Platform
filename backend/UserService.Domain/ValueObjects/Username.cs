using System.Text.RegularExpressions;
using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace UserService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Username : IRegexString
{
    public static int MinLength => 3;
    public static int MaxLength => 64;

    public static string RegexValidationError =>
        "Use only underscores (cannot be at the start/end or be repeated), lowercase Latin letters, and numbers";

    [GeneratedRegex("^[a-z0-9]+(_[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex GetGeneratedRegex();

    public static Regex Regex { get; } = GetGeneratedRegex();

    private static Validation Validate(in string value) => ValidationHelper.PatternStringValidate<Username>(value);
}