using SharedKernel.Domain.Helpers;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct ForumTitle : IVogen<ForumTitle, string>
{
    public const int MinLength = 3;
    public const int MaxLength = 64;
    private static Validation Validate(in string value) => ValidationHelper.StringValidate(value, MinLength, MaxLength);
    private static string NormalizeInput(string input) => input.Trim();
}