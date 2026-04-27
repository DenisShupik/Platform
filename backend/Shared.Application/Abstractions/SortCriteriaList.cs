using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Interfaces;

namespace Shared.Application.Abstractions;

public sealed class SortCriteriaList<T> : List<SortCriteria<T>>, IReferenceTypeWithTryParseExtended<SortCriteriaList<T>>
    where T : Enum
{
    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteriaList<T>? result)
    {
        throw new NotImplementedException("Use [GenerateBind]");
    }
    
    public static bool TryParseExtended(string? input, [NotNullWhen(true)] out SortCriteriaList<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrEmpty(input))
        {
            result = null;
            error = "Cannot parse empty value";
            return false;
        }

        var list = new SortCriteriaList<T>();
        var span = input.AsSpan();
        var offset = 0;
        var length = span.Length;

        while (offset < length)
        {
            var commaOffset = span[offset..].IndexOf(',');
            ReadOnlySpan<char> token;
            if (commaOffset == -1)
            {
                token = span.Slice(offset, length - offset);
                offset = length;
            }
            else
            {
                token = span.Slice(offset, commaOffset);
                offset += commaOffset + 1;
            }

            if (SortCriteria<T>.TryParseExtended(token, out var parsed, out error))
            {
                list.Add(parsed.Value);
            }
            else
            {
                result = null;
                return false;
            }
        }

        result = list;
        error = null;
        return true;
    }

    public static SortCriteriaList<T> Default { get; } = [];
}