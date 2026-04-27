using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Result<TValue1, TError1> : IResult
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


    public bool SuccessOrErrors([NotNullWhen(false)] out TError1? error)
    {
        if (_error == null)
        {
            error = null;
            return true;
        }
        
        error = _error;
        return false;
    }
    
    public bool ValueOrErrors(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out TError1? error)
    {
        if (_error == null)
        {
            value = _value;
            error = null;
            return true;
        }

        value = default;
        error = _error;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1
    ) =>
        _error == null ? f0(_value) : f1(_error);

    public void Apply(
        Action<TValue1> f0,
        Action<TError1> f1
    )
    {
        if (_error == null)
        {
            f0(_value);
            return;
        }

        f1(_error!);
    }
}