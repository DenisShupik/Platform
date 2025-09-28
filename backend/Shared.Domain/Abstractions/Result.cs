using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shared.Domain.Errors;

namespace Shared.Domain.Abstractions;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Success : IEquatable<Success>, IComparable<Success>
{
    public static readonly Success Instance = new();

    public int CompareTo(Success other) => 0;

    public bool Equals(Success other) => true;

    public override bool Equals(object? obj) => obj is Success;

    public override int GetHashCode() => 0;

    public override string ToString() => nameof(Success);
}

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

public readonly struct Result<TValue1, TError1, TError2>
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

    public bool TryPickOrExtend<TError3>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue1, TError3, TError1, TError2>? extendedValue)
        where TError3 : Error

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
            _ => throw new InvalidOperationException()
        };

        return false;
    }

    public bool TryPickOrExtend<TValue2, TError3>(
        [NotNullWhen(true)] out TValue1? value,
        [NotNullWhen(false)] out Result<TValue2, TError1, TError2, TError3>? extendedValue)
        where TValue2 : notnull
        where TError3 : Error
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
            _ => throw new InvalidOperationException()
        };

        return false;
    }
}

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

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4>
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
    
    public bool TryPickOrExtend<TValue2, TError5>(
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
}

public readonly struct Result<TValue1, TError1, TError2, TError3, TError4, TError5>
    where TValue1 : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
    where TError4 : Error
    where TError5 : Error
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

    public TResult Match<TResult>(
        Func<TValue1, TResult> f0,
        Func<TError1, TResult> f1,
        Func<TError2, TResult> f2,
        Func<TError3, TResult> f3,
        Func<TError4, TResult> f4,
        Func<TError5, TResult> f5
    ) =>
        _index switch
        {
            0 => f0(_value),
            1 => f1((TError1)_error!),
            2 => f2((TError2)_error!),
            3 => f3((TError3)_error!),
            4 => f4((TError4)_error!),
            5 => f5((TError5)_error!),
            _ => throw new InvalidOperationException()
        };
}