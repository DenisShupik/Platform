using System.Diagnostics.CodeAnalysis;
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
            error = "Cannot parse empty value";
            return false;
        }

        P value;
        try
        {
            value = P.Parse(input, null);
        }
        catch
        {
            result = null;
            error = "Cannot parse input value";
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
        P value;
        try
        {
            value = P.Parse(input, null);
        }
        catch
        {
            result = null;
            error = "Cannot parse input value";
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
        try
        {
            result = Enum.Parse<T>(input, true);
            error = null;
            return true;
        }
        catch
        {
            error = "Cannot parse input value";
            result = null;
            return false;
        }
    }
}