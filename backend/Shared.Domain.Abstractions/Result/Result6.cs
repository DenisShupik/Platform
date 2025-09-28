using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions;

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4, TError5>
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
    where TError5 : Error
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

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TValue1 value) =>
        new(value);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TError1 error) =>
        new(error, 1);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TError2 error) =>
        new(error, 2);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TError3 error) =>
        new(error, 3);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TError4 error) =>
        new(error, 4);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(TError5 error) =>
        new(error, 5);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4, TError5>(
        Result<TValue1, TError5> result)
        => result.Error == null ? result.Value : result.Error;

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3,
        Func<TError4, TResult> f4,
        Func<TError5, TResult> f5
    ) =>
        Index switch
        {
            0 => f0(Value),
            1 => f1((TError1)Error!),
            2 => f2((TError2)Error!),
            3 => f3((TError3)Error!),
            4 => f4((TError4)Error!),
            5 => f5((TError5)Error!),
            _ => throw new InvalidOperationException()
        };
}