using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct PostContent : INonEmptyString
{
    public static int MinLength => 2;
    public static int MaxLength => 1024;
    private static Validation Validate(in string value) => ValidationHelper.NonEmptyStringValidate<PostContent>(value);
    private static string NormalizeInput(string input) => input.Trim();
}