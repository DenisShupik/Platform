using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
    where TError5 : Error
    where TError6 : Error
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

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TValue1 value) =>
        new(value);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError1 error) =>
        new(error, 1);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError2 error) =>
        new(error, 2);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError3 error) =>
        new(error, 3);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError4 error) =>
        new(error, 4);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError5 error) =>
        new(error, 5);

    public static implicit operator
        Result<TValue1, TError1, TError2, TError3, TError4, TError5, TError6>(TError6 error) =>
        new(error, 6);

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3,
        Func<TError4, TResult> f4,
        Func<TError5, TResult> f5,
        Func<TError6, TResult> f6
    ) =>
        Index switch
        {
            0 => f0(Value),
            1 => f1((TError1)Error!),
            2 => f2((TError2)Error!),
            3 => f3((TError3)Error!),
            4 => f4((TError4)Error!),
            5 => f5((TError5)Error!),
            6 => f6((TError6)Error!),
            _ => throw new InvalidOperationException()
        };

    public bool TryOrMapError<TValue2>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4, TError5, TError6>? mappedResult)
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
            4 => (TError4)Error!,
            5 => (TError5)Error!,
            6 => (TError6)Error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}