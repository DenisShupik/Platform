using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct ForumSanctionReason : INonEmptyString, IHasTryFrom<ForumSanctionReason, string>
{
    public static int MinLength => 3;
    public static int MaxLength => 500;

    private static Validation Validate(in string value) =>
        ValidationHelper.NonEmptyStringValidate<ForumSanctionReason>(value);

    private static string NormalizeInput(string input) => input.Trim();
}
