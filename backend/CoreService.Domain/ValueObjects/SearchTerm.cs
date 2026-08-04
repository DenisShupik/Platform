using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

/// <summary>
/// Поисковый запрос
/// </summary>
[ValueObject<string>]
public readonly partial struct SearchTerm : INonEmptyString, IHasTryFrom<SearchTerm, string>
{
    public static int MinLength => 2;
    public static int MaxLength => 100;

    private static Validation Validate(in string value) => ValidationHelper.NonEmptyStringValidate<SearchTerm>(value);
    private static string NormalizeInput(string input) => input.Trim();
}
