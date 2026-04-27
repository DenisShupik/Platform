using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Errors<TError1, TError2>
    where TError1 : Error
    where TError2 : Error
{
    internal readonly byte Index;
    internal readonly Error Error;

    internal Errors(Error error, byte index)
    {
        Index = index;
        Error = error;
    }

    public static implicit operator Errors<TError1, TError2>(TError1 error) => new(error, 1);
    public static implicit operator Errors<TError1, TError2>(TError2 error) => new(error, 2);
}

public readonly struct Result<TValue1, TError1, TError2> : IResult
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
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

    public static implicit operator Result<TValue1, TError1, TError2>(TValue1 value) => new(value);
    public static implicit operator Result<TValue1, TError1, TError2>(TError1 error) => new(error, 1);
    public static implicit operator Result<TValue1, TError1, TError2>(TError2 error) => new(error, 2);

    public static implicit operator Result<TValue1, TError1, TError2>(Errors<TError1, TError2> errors) =>
        new(errors.Error, errors.Index);

    public bool GetValue([NotNullWhen(true)] out TValue1? value)
    {
        if (_index == 0)
        {
            value = _value;
            return true;
        }

        value = default;
        return false;
    }

    public bool SuccessOrErrors([NotNullWhen(false)] out Errors<TError1, TError2>? errors)
    {
        if (_index == 0)
        {
            errors = null;
            return true;
        }

        errors = new Errors<TError1, TError2>(_error!, _index);
        return false;
    }

    public bool ValueOrErrors([NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Errors<TError1, TError2>? errors)
    {
        if (_index == 0)
        {
            value = _value;
            errors = null;
            return true;
        }

        value = default;
        errors = new Errors<TError1, TError2>(_error!, _index);
        return false;
    }

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2
    ) =>
        _index switch
        {
            0 => f0(_value),
            1 => f1((TError1)_error!),
            2 => f2((TError2)_error!),
            _ => throw new InvalidOperationException()
        };

    public void Apply(
        Action<TValue1> f0,
        Action<TError1> f1,
        Action<TError2> f2
    )
    {
        switch (_index)
        {
            case 0:
                f0(_value);
                break;
            case 1:
                f1((TError1)_error!);
                break;
            case 2:
                f2((TError2)_error!);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}