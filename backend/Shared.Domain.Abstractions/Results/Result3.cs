using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Result<TValue1, TError1, TError2> : IResult
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
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

    public static implicit operator Result<TValue1, TError1, TError2>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1, TError2>(TError1 error) => new(error, 1);
    public static implicit operator Result<TValue1, TError1, TError2>(TError2 error) => new(error, 2);

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2
    ) =>
        Index switch
        {
            0 => f0(Value),
            1 => f1((TError1)Error!),
            2 => f2((TError2)Error!),
            _ => throw new InvalidOperationException()
        };

    public bool TryGetOrMap<TValue2>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2>? extendedValue)
        where TValue2 : notnull

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
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryPickOrExtend<TError3>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue1, TError3, TError1, TError2>? extendedValue)
        where TError3 : Error

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
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryGetOrExtend<TValue2, TError3>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3>? extendedValue)
        where TValue2 : notnull
        where TError3 : Error
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
            _ => throw new InvalidOperationException()
        };

        return false;
    }
    
    public bool TryOrExtend<TValue2, TError3>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3>? extendedValue)
        where TValue2 : notnull
        where TError3 : Error
    {
        if (Index == 0)
        {
            extendedValue = null;
            return true;
        }

        extendedValue = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}