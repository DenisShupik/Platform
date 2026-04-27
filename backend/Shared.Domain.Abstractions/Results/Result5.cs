using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Abstractions.Results;

public readonly struct Errors<TError1, TError2, TError3, TError4>
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
{
    internal readonly byte Index;
    internal readonly Error Error;

    private Errors(Error error, byte index)
    {
        Index = index;
        Error = error;
    }

    public static implicit operator Errors<TError1, TError2, TError3, TError4>(TError1 error) => new(error, 1);
    public static implicit operator Errors<TError1, TError2, TError3, TError4>(TError2 error) => new(error, 2);
    public static implicit operator Errors<TError1, TError2, TError3, TError4>(TError3 error) => new(error, 3);
    public static implicit operator Errors<TError1, TError2, TError3, TError4>(TError4 error) => new(error, 4);
}

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4> : IResult
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
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

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TValue1 value) =>
        new(value);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError1 error) =>
        new(error, 1);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError2 error) =>
        new(error, 2);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError3 error) =>
        new(error, 3);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(TError4 error) =>
        new(error, 4);

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(
        Errors<TError3, TError4> errors) => new(errors.Error, (byte)(errors.Index + 2));

    public static implicit operator Result<TValue1, TError1, TError2, TError3, TError4>(
        Errors<TError2, TError3, TError4> errors) => new(errors.Error, (byte)(errors.Index + 1));

    public bool IsSuccess => _error == null;

    public bool IsError => _error != null;

    public bool SuccessOrErrors([NotNullWhen(false)] out Errors<TError1, TError2, TError3, TError4>? errors)
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
            4 => (TError4)_error!,
            _ => throw new InvalidOperationException()
        };
        return false;
    }

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3,
        Func<TError4, TResult> f4
    ) =>
        _index switch
        {
            0 => f0(_value),
            1 => f1((TError1)_error!),
            2 => f2((TError2)_error!),
            3 => f3((TError3)_error!),
            4 => f4((TError4)_error!),
            _ => throw new InvalidOperationException()
        };

    public bool TryGetOrMap<TValue2>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4>? mappedResult)
        where TValue2 : notnull
    {
        if (_index == 0)
        {
            value = _value;
            mappedResult = null;
            return true;
        }

        value = default;

        mappedResult = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            4 => (TError4)_error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryOrMap<TValue2>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4>? mappedResult)
        where TValue2 : notnull
    {
        if (_index == 0)
        {
            mappedResult = null;
            return true;
        }

        mappedResult = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            4 => (TError4)_error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryGetOrExtend<TValue2, TError5>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4, TError5>? extendedValue)
        where TValue2 : notnull
        where TError5 : Error
    {
        if (_index == 0)
        {
            value = _value;
            extendedValue = null;
            return true;
        }

        value = default;

        extendedValue = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            4 => (TError4)_error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryOrExtend<TValue2, TError5>(
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3, TError4, TError5>? extendedValue)
        where TValue2 : notnull
        where TError5 : Error
    {
        if (_index == 0)
        {
            extendedValue = null;
            return true;
        }

        extendedValue = _index switch
        {
            1 => (TError1)_error!,
            2 => (TError2)_error!,
            3 => (TError3)_error!,
            4 => (TError4)_error!,
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}