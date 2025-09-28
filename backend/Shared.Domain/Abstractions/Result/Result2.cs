using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Shared.Domain.Errors;

namespace Shared.Domain.Abstractions;

public readonly struct Result<TValue1, TError1>
    where TValue1 : notnull
    where TError1 : Error
{
    private readonly TValue1 _value;
    private readonly TError1? _error;

    private Result(TValue1 value)
    {
        _value = value;
        _error = null;
    }

    private Result(TError1 error)
    {
        _value = default!;
        _error = error;
    }

    public static implicit operator Result<TValue1, TError1>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1>(TError1 error) => new(error);

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1
    ) =>
        _error == null ? f0(_value) : f1(_error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TValue1, TError2, TError3, TError1> Extend<TError2, TError3>()
        where TError2 : Error
        where TError3 : Error
        => _error == null ? _value : _error;

    public bool TryPick(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out TError1? extendedValue)
    {
        if (_error == null)
        {
            value = _value;
            extendedValue = null;
            return true;
        }

        value = default;
        extendedValue = _error;
        return false;
    }
}