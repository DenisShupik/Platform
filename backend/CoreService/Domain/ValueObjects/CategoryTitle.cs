using SharedKernel.Domain.Helpers;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct CategoryTitle : IVogen<CategoryTitle, string>
{
    public const int MinLength = 3;
    public const int MaxLength = 128;
    private static Validation Validate(in string value) => ValidationHelper.StringValidate(value, MinLength, MaxLength);
    private static string NormalizeInput(string input) => input.Trim();
}