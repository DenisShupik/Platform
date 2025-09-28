using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions;

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4>
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
{
    internal readonly TValue1 Value;
    internal readonly byte Index;
    internal readonly Error? Error;

    private Result(TValue1 value)
    {
        Value = value;
        Error = null;
        Index = 0;
    }

    private Result(Error error, byte index)
    {
        Value = default!;
        Error = error;
        Index = index;
    }

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TValue1 value) =>
        new(value);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError1 error) =>
        new(error, 1);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError2 error) =>
        new(error, 2);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError3 error) =>
        new(error, 3);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError4 error) =>
        new(error, 4);

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3,
        Func<TError4, TResult> f4
    ) =>
        Index switch
        {
            0 => f0(Value),
            1 => f1((TError1)Error!),
            2 => f2((TError2)Error!),
            3 => f3((TError3)Error!),
            4 => f4((TError4)Error!),
            _ => throw new InvalidOperationException()
        };

    public bool TryPickOrExtend<TValue2, TError5>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4, TError5>? extendedValue)
        where TValue2 : notnull
        where TError5 : Error
    {
        if (Index == 0)
        {
            value = Value;
            extendedValue = null;
            return true;
        }

        value = default;

        extendedValue = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            3 => (TError3)Error!,
            4 => (TError4)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}