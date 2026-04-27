using System.Runtime.CompilerServices;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct PostId : IId, IHasTryFrom<PostId, Guid>
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(PostId left, PostId right) => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(PostId left, PostId right) => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(PostId left, PostId right) => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(PostId left, PostId right) => left.Value >= right.Value;
}