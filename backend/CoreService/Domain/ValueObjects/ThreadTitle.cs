using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct ThreadTitle : IVogen<ThreadTitle, string>, IHasMinLength, IHasMaxLength
{
    public static int MinLength => 3;
    public static int MaxLength => 128;
    private static Validation Validate(in string value) => ValidationHelper.StringValidate<ThreadTitle>(value);
    private static string NormalizeInput(string input) => input.Trim();
}