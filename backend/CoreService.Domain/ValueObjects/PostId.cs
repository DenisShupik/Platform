using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct PostId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);

    public static bool operator <(PostId a, PostId b)
    {
        return a.Value < b.Value;
    }

    public static bool operator >(PostId a, PostId b)
    {
        return a.Value > b.Value;
    }
}