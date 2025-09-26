using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;

namespace Shared.Domain.Abstractions;

[CollectionBuilder(typeof(EnumSetBuilder), nameof(EnumSetBuilder.Create))]
public sealed class EnumSet<T> :
    HashSet<T>, IReferenceTypeWithTryParseExtended<EnumSet<T>>
    where T : struct, Enum
{
    private const string ErrorMessage = $"{nameof(EnumSet<>)} must contain at least one element";

    // TODO: не используется, но если убрать то linq2db не сможет работать с этом типом, поискать решение как сделать приватным
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EnumSet()
    {
        throw new NotImplementedException("Internal only for linq2Db");
    }

    private EnumSet(IEnumerable<T> collection) : base(collection)
    {
    }

    internal EnumSet(ReadOnlySpan<T> values)
    {
        if (values.Length == 0) throw new ValidationException(ErrorMessage);
        foreach (var value in values)
        {
            Add(value);
        }
    }

    public EnumSet(HashSet<T> collection) : base(
        collection.Count == 0
            ? throw new ValidationException(ErrorMessage)
            : collection
    )
    {
    }

    public static bool TryCreate(HashSet<T> collection, [NotNullWhen(true)] out EnumSet<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        if (collection.Count == 0)
        {
            result = null;
            error = ErrorMessage;
            return false;
        }

        result = new EnumSet<T>(collection);
        error = null;
        return true;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out EnumSet<T>? result)
    {
        throw new NotImplementedException("Use [GenerateBind]");
    }

    public static bool TryParseExtended(string? input, [NotNullWhen(true)] out EnumSet<T>? result,
        [NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = null;
            error = "Cannot parse empty value";
            return false;
        }

        var span = input.AsSpan();
        var length = span.Length;
        var offset = 0;
        EnumSet<T>? set = null;

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

            if (ParseExtendedHelper.TryParseExtended<T>(token, out var parsed, out error))
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
            error = "Cannot parse empty value";
            return false;
        }

        result = set;
        error = null;
        return true;
    }

    public static EnumSet<T> Default { get; } = [];
}

public static class EnumSetBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EnumSet<T> Create<T>(ReadOnlySpan<T> values) where T : struct, Enum => new(values);
}