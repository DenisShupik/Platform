using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace UserService.Domain.ValueObjects;

/// <summary>
/// Нормализованный фрагмент имени пользователя для поиска.
/// </summary>
[ValueObject<string>]
public readonly partial struct UsernameSearchTerm : INonEmptyString, IHasTryFrom<UsernameSearchTerm, string>
{
    public static int MinLength => 1;
    public static int MaxLength => Username.MaxLength;

    private static Validation Validate(in string value) =>
        ValidationHelper.NonEmptyStringValidate<UsernameSearchTerm>(value);

    private static string NormalizeInput(string input) => input.Trim().ToLowerInvariant();
}
