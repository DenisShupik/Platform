using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<long>]
public readonly partial struct PostId : IId
{
    private static Validation Validate(in long value) => ValidationHelper.LongValidate(value);
    public static bool operator <(PostId left, PostId right) => left.Value < right.Value;
    public static bool operator >(PostId left, PostId right) => left.Value > right.Value;
    public PostId Increment() => From(Value + 1);
}