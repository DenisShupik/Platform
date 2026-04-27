using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct RoleTitle : INonEmptyString, IHasTryFrom<RoleTitle, string>
{
    public static int MinLength => 3;
    public static int MaxLength => 32;

    private static Validation Validate(in string value) =>
        ValidationHelper.NonEmptyStringValidate<RoleTitle>(value);

    private static string NormalizeInput(string input) => input.Trim();
}