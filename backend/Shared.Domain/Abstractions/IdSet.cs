using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;

namespace Shared.Domain.Abstractions;

public sealed class IdSet<T, P> : HashSet<T>, IReferenceTypeWithTryParseExtended<IdSet<T, P>>
    where T : struct, IId, IHasTryFrom<T, P>, IVogen<T, P>
    where P : ISpanParsable<P>
{
    private static readonly string ErrorMessage =
        ValidationErrorCodec.Encode(ValidationErrorCodes.CollectionMustNotBeEmpty);

    private IdSet()
    {
    }

    private IdSet(IEnumerable<T> collection) : base(collection)
    {
    }

    public IdSet(HashSet<T> collection) : base(
        collection.Count == 0
            ? throw new ValidationException(ErrorMessage)
            : collection
    )
    {
    }

    public static IdSet<T, P>? TryCreate<V>(Dictionary<T, V>.KeyCollection collection)
    {
        return collection.Count == 0 ? null : new IdSet<T, P>(collection);
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out IdSet<T, P>? result)
    {
        throw new NotImplementedException("Use [GenerateBind]");
    }

    public static bool TryParseExtended(string? input, [NotNullWhen(true)] out IdSet<T, P>? result,
        [NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseEmptyValue);
            return false;
        }

        var span = input.AsSpan();
        var length = span.Length;
        var offset = 0;
        IdSet<T, P>? set = null;

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

            token = token.Trim();

            if (ParseExtendedHelper.TryParseExtended<T, P>(token, out var parsed, out error))
            {
                set ??= [];
                set.Add(parsed.Value);
            }
            else
            {
                result = null;
                return false;
            }
        }

        if (set is null || set.Count == 0)
        {
            result = null;
            error = ValidationErrorCodec.Encode(ValidationErrorCodes.CannotParseEmptyValue);
            return false;
        }

        result = set;
        error = null;
        return true;
    }

    public static IdSet<T, P> Default { get; } = [];
}
