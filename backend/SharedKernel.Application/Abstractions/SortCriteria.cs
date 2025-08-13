using System.ComponentModel.DataAnnotations;
using SharedKernel.Application.Enums;

namespace SharedKernel.Application.Abstractions;

public readonly record struct SortCriteria<T>
    where T : Enum
{
    public required T Field { get; init; }
    public required SortOrderType Order { get; init; }

    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ValidationException("Parameter cannot be null or empty.");
        }

        if (trimmed[0] != '-')
        {
            result = new SortCriteria<T>
            {
                Field = (T)Enum.Parse(typeof(T), trimmed, true),
                Order = SortOrderType.Ascending
            };
        }
        else
        {
            result = new SortCriteria<T>
            {
                Field = (T)Enum.Parse(typeof(T), trimmed[1..], true),
                Order = SortOrderType.Descending
            };
        }

        return true;
    }
}