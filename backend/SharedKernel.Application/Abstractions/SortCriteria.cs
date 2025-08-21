using SharedKernel.Application.Enums;

namespace SharedKernel.Application.Abstractions;

public readonly record struct SortCriteria<T>
    where T : Enum
{
    public required T Field { get; init; }
    public required SortOrderType Order { get; init; }

    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        var token = value?.Trim();

        if (string.IsNullOrEmpty(token))
        {
            result = default;
            return false;
        }

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

        return true;
    }

    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        var token = value.Trim();

        if (token.IsEmpty)
        {
            result = default;
            return false;
        }

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

        return true;
    }
}