using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Errors;
using Shared.Domain.Interfaces;

namespace Shared.Domain.Helpers;

public static class ParseExtendedHelper
{
    public static bool TryParseExtended<T, P>(
        string? input,
        [NotNullWhen(true)] out T? result,
        [NotNullWhen(false)] out string? error
    )
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (string.IsNullOrEmpty(input))
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseEmptyValue);
            return false;
        }

        if (!P.TryParse(input, null, out var value))
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseInputValue);
            return false;
        }

        var maybeResult = T.TryFrom(value);
        if (!maybeResult.IsSuccess)
        {
            result = null;
            error = maybeResult.Error.ErrorMessage;
            return false;
        }

        result = maybeResult.ValueObject;
        error = null;
        return true;
    }

    public static bool TryParseExtended<T, P>(
        ReadOnlySpan<char> input,
        [NotNullWhen(true)] out T? result,
        [NotNullWhen(false)] out string? error
    )
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!P.TryParse(input, null, out var value))
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseInputValue);
            return false;
        }

        var maybeResult = T.TryFrom(value);
        if (!maybeResult.IsSuccess)
        {
            result = null;
            error = maybeResult.Error.ErrorMessage;
            return false;
        }

        result = maybeResult.ValueObject;
        error = null;
        return true;
    }

    public static bool TryParseExtended<T>(
        ReadOnlySpan<char> input,
        [NotNullWhen(true)] out T? result,
        [NotNullWhen(false)] out string? error
    )
        where T : struct, Enum
    {
        if (!Enum.TryParse<T>(input, true, out var value))
        {
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseInputValue);
            result = null;
            return false;
        }

        result = value;
        error = null;
        return true;
    }
}
