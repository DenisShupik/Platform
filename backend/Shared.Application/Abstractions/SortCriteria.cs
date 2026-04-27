using System;
using System.Diagnostics.CodeAnalysis;
using Shared.Application.Enums;
using Shared.Domain.Interfaces;

namespace Shared.Application.Abstractions;

public readonly record struct SortCriteria<T> : IValueTypeWithTryParseExtended<SortCriteria<T>>
    where T : Enum
{
    public required T Field { get; init; }
    public required SortOrderType Order { get; init; }
    
    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        throw new NotImplementedException("Use [GenerateBind]");
    }

    public static bool TryParseExtended(ReadOnlySpan<char> input, [NotNullWhen(true)] out SortCriteria<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        try
        {
            if (input[0] != '-')
            {
                result = new SortCriteria<T>
                {
                    Field = (T)Enum.Parse(typeof(T), input, true),
                    Order = SortOrderType.Ascending
                };
            }
            else
            {
                result = new SortCriteria<T>
                {
                    Field = (T)Enum.Parse(typeof(T), input[1..], true),
                    Order = SortOrderType.Descending
                };
            }

            error = null;
            return true;
        }
        catch
        {
            result = null;
            error = "Cannot parse input value";
            return false;
        }
    }

    public static bool TryParseExtended(string? input, [NotNullWhen(true)] out SortCriteria<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        var token = input?.Trim();

        if (string.IsNullOrEmpty(token))
        {
            result = null;
            error = "Cannot parse empty value";
            return false;
        }

        try
        {
            if (token[0] != '-')
            {
                result = new SortCriteria<T>
                {
                    Field = (T)Enum.Parse(typeof(T), token, true),
                    Order = SortOrderType.Ascending
                };
            }
            else
            {
                result = new SortCriteria<T>
                {
                    Field = (T)Enum.Parse(typeof(T), token[1..], true),
                    Order = SortOrderType.Descending
                };
            }

            error = null;
            return true;
        }
        catch
        {
            result = null;
            error = "Cannot parse input value";
            return false;
        }
    }
}