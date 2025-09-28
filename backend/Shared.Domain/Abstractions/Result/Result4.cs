using Shared.Domain.Errors;

namespace Shared.Domain.Abstractions;

public readonly struct Result<TValue1, TError1, TError2, TError3>
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
{
    private readonly TValue1 _value;
    private readonly byte _index;
    private readonly Error? _error;

    private Result(TValue1 value)
    {
        _value = value;
        _error = null;
        _index = 0;
    }

    private Result(Error error, byte index)
    {
        _value = default!;
        _error = error;
        _index = index;
    }

    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError1 error) => new(error, 1);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError2 error) => new(error, 2);
    public static implicit operator Result<TValue1, TError1, TError2, TError3>(TError3 error) => new(error, 3);

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3
    ) =>
        _index switch
        {
            0 => f0(_value),
            1 => f1((TError1)_error!),
            2 => f2((TError2)_error!),
            3 => f3((TError3)_error!),
            _ => throw new InvalidOperationException()
        };
}