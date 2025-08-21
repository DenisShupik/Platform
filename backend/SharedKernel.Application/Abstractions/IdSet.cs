using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Application.Abstractions;

public sealed class IdSet<T> : HashSet<T> where T : IId, ISpanParsable<T>
{
    private IdSet()
    {
    }

    private IdSet(IEnumerable<T> collection) : base(collection)
    {
    }

    public static IdSet<T> Create(IEnumerable<T> collection)
    {
        var result = new IdSet<T>(collection);
        return result.Count == 0 ? throw new ArgumentException(nameof(collection)) : result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out IdSet<T>? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var span = value.AsSpan();
        var length = span.Length;
        var offset = 0;
        IdSet<T>? set = null;

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

            if (token.Length == 0 || !T.TryParse(token, provider, out var parsed)) return false;


            set ??= [];
            set.Add(parsed);
        }

        if (set is null || set.Count == 0) return false;

        result = set;
        return true;
    }
}