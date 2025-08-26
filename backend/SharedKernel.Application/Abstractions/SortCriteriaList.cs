namespace SharedKernel.Application.Abstractions;

public sealed class SortCriteriaList<T> : List<SortCriteria<T>> where T : Enum
{
    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteriaList<T>? result)
    {
        result = null;

        if (string.IsNullOrEmpty(value)) return false;

        var list = new SortCriteriaList<T>();
        var span = value.AsSpan();
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

            if (!SortCriteria<T>.TryParse(token, provider, out var parsed)) return false;

            list.Add(parsed);
        }

        result = list;
        return true;
    }
}