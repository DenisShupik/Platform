using System;
using System.Diagnostics.CodeAnalysis;
using Shared.Application.Enums;
using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;

namespace Shared.Application.Abstractions;

public readonly record struct SortCriteria<T> : IValueTypeWithTryParseExtended<SortCriteria<T>>
    where T : struct, Enum
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
        var descending = !input.IsEmpty && input[0] == '-';
        var fieldInput = descending ? input[1..] : input;
        if (!fieldInput.IsEmpty && Enum.TryParse<T>(fieldInput, true, out var field))
        {
            result = new SortCriteria<T>
            {
                Field = field,
                Order = descending ? SortOrderType.Descending : SortOrderType.Ascending
            };
            error = null;
            return true;
        }

        result = null;
        error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseInputValue);
        return false;
    }

    public static bool TryParseExtended(string? input, [NotNullWhen(true)] out SortCriteria<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseEmptyValue);
            return false;
        }

        return TryParseExtended(input.AsSpan().Trim(), out result, out error);
    }
}
