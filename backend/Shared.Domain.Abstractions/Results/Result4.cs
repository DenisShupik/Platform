using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Result<TValue1, TError1, TError2, TError3> : IResult
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
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

    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError1 error) => new(error, 1);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError2 error) => new(error, 2);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError3 error) => new(error, 3);

    public static implicit operator Result<TValue1, TError1, TError2, TError3>(Result<TValue1, TError1> result)
        => result.Error == null ? result.Value : result.Error;

    public static implicit operator Result<TValue1, TError1, TError2, TError3>(Result<TValue1, TError3> result)
        => result.Error == null ? result.Value : result.Error;

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3
    ) =>
        Index switch
        {
            0 => f0(Value),
            1 => f1((TError1)Error!),
            2 => f2((TError2)Error!),
            3 => f3((TError3)Error!),
            _ => throw new InvalidOperationException()
        };
    
    public void Apply(
        Action<TValue1> f0,
        Action<TError1> f1,
        Action<TError2> f2,
        Action<TError3> f3
    )
    {
        switch (Index)
        {
            case 0:
               f0(Value);
                break;
            case 1:
                f1((TError1)Error!);
                break;
            case 2:
                f2((TError2)Error!);
                break;
            case 3:
                f3((TError3)Error!);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public bool TryGetOrMap<TValue2>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3>? mappedResult)
        where TValue2 : notnull
    {
        if (Index == 0)
        {
            value = Value;
            mappedResult = null;
            return true;
        }

        value = default;

        mappedResult = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            3 => (TError3)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryOrMap<TValue2>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3>? mappedResult)
        where TValue2 : notnull
    {
        if (Index == 0)
        {
            mappedResult = null;
            return true;
        }

        mappedResult = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            3 => (TError3)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryGetOrExtend<TValue2, TError4>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4>? extendedResult)
        where TValue2 : notnull
        where TError4 : Error
    {
        if (Index == 0)
        {
            value = Value;
            extendedResult = null;
            return true;
        }

        value = default;

        extendedResult = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            3 => (TError3)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
    
    public bool TryOrExtend<TValue2, TError4>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4>? extendedResult)
        where TValue2 : notnull
        where TError4 : Error
    {
        if (Index == 0)
        {
            extendedResult = null;
            return true;
        }
        
        extendedResult = Index switch
        {
            1 => (TError1)Error!,
            2 => (TError2)Error!,
            3 => (TError3)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}