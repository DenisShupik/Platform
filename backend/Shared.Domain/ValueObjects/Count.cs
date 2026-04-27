using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace Shared.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct Count : IHasTryFrom<Count, int>
{
    public static readonly Count Default = From(0);
    private static Validation Validate(in int value) => ValidationHelper.CountValidate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Count left, int right) => left.Value < right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Count left, int right) => left.Value > right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Count Increment() => From(checked(Value + 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public Count Decrement() => From(checked(Value - 1));
}