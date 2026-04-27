using System.Runtime.InteropServices;

namespace Shared.Domain.Abstractions;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Success : IEquatable<Success>, IComparable<Success>
{
    public static readonly Success Instance = new();

    public int CompareTo(Success other) => 0;

    public bool Equals(Success other) => true;

    public override bool Equals(object? obj) => obj is Success;

    public override int GetHashCode() => 0;

    public override string ToString() => nameof(Success);
}