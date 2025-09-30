using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Result<TValue1, TError1>
    where TValue1 : notnull
    where TError1 : Error
{
    internal readonly TValue1 Value;
    internal readonly TError1? Error;

    private Result(TValue1 value)
    {
        Value = value;
        Error = null;
    }

    private Result(TError1 error)
    {
        Value = default!;
        Error = error;
    }

    public static implicit operator Result<TValue1, TError1>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1>(TError1 error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1
    ) =>
        Error == null ? f0(Value) : f1(Error);

    public bool TryGet(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out TError1? error)
    {
        if (Error == null)
        {
            value = Value;
            error = null;
            return true;
        }

        value = default;
        error = Error;
        return false;
    }
}