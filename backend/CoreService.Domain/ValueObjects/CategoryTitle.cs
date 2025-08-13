using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct CategoryTitle : INonEmptyString
{
    public static int MinLength => 3;
    public static int MaxLength => 128;
    private static Validation Validate(in string value) => ValidationHelper.NonEmptyStringValidate<CategoryTitle>(value);
    private static string NormalizeInput(string input) => input.Trim();
}