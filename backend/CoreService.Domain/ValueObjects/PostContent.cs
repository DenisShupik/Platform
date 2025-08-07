using ProtoBuf;
using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
[ProtoContract(Surrogate = typeof(string))]
public readonly partial struct PostContent : IVogen<PostContent, string>, INonEmptyString
{
    public static int MinLength => 2;
    public static int MaxLength => 1024;
    private static Validation Validate(in string value) => ValidationHelper.NonEmptyStringValidate<PostContent>(value);
    private static string NormalizeInput(string input) => input.Trim();
}