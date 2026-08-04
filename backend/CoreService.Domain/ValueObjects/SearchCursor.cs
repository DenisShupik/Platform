using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

/// <summary>
/// Непрозрачный курсор постраничной выдачи поиска.
/// </summary>
[ValueObject<string>]
public readonly partial struct SearchCursor : INonEmptyString, IHasTryFrom<SearchCursor, string>
{
    public static int MinLength => 1;
    public static int MaxLength => 512;

    private static Validation Validate(in string value) =>
        ValidationHelper.NonEmptyStringValidate<SearchCursor>(value);
}
