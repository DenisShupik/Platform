using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Errors<TError1, TError2, TError3>
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
{
    internal readonly byte Index;
    internal readonly Error Error;

    private Errors(Error error, byte index)
    {
        Index = index;
        Error = error;
    }

    public static implicit operator Errors<TError1, TError2, TError3>(TError1 error) => new(error, 1);
    public static implicit operator Errors<TError1, TError2, TError3>(TError2 error) => new(error, 2);
    public static implicit operator Errors<TError1, TError2, TError3>(TError3 error) => new(error, 3);
}

public readonly struct Result<TValue1, TError1, TError2, TError3> : IResult
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

    public static implicit operator Result<TValue1, TError1, TError2, TError3>(Errors<TError1, TError2> errors) =>
        new(errors.Error, errors.Index);

    public bool ValueOrErrors([NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Errors<TError1, TError2, TError3>? errors)
    {
        if (_index == 0)
        {
            value = _value;
            errors = null;
            return true;
        }

        value = default;
        errors = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            _ => throw new InvalidOperationException()
        };
        return false;
    }
    
    public bool SuccessOrErrors([NotNullWhen(false)] out Errors<TError1, TError2, TError3>? errors)
    {
        if (_index == 0)
        {
            errors = null;
            return true;
        }

        errors = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            _ => throw new InvalidOperationException()
        };
        return false;
    }

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

    public void Apply(
        Action<TValue1> f0,
        Action<TError1> f1,
        Action<TError2> f2,
        Action<TError3> f3
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
            case 3:
                f3((TError3)_error!);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}