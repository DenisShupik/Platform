using Shared.Domain.Errors;

namespace Shared.Domain.Abstractions;

public readonly struct Result<TValue, TError1, TError2>
    where TError1 : Error
    where TError2 : Error
{
    private readonly TValue _value;
    private readonly byte _index;
    private readonly Error? _error;

    private Result(TValue value)
    {
        _value = value;
        _error = null;
        _index = 0;
    }

    private Result(Error? error, byte index)
    {
        _value = default!;
        _error = error;
        _index = index;
    }

    public static implicit operator Result<TValue, TError1, TError2>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError1, TError2>(TError1? error) => new(error, 1);
    public static implicit operator Result<TValue, TError1, TError2>(TError2? error) => new(error, 2);
}