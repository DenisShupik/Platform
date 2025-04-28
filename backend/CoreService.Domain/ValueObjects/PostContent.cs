using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct PostContent : IVogen<PostContent, string>, IHasMinLength, IHasMaxLength
{
    public static int MinLength => 2;
    public static int MaxLength => 1024;
    private static Validation Validate(in string value) => ValidationHelper.StringValidate<PostContent>(value);
    private static string NormalizeInput(string input) => input.Trim();
}