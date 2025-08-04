using System.ComponentModel.DataAnnotations;
using SharedKernel.Application.Enums;

namespace SharedKernel.Application.Abstractions;

public readonly record struct SortCriteria<T>
    where T : Enum
{
    public readonly T Field;
    public readonly SortOrderType Order;

    private SortCriteria(T field, SortOrderType order)
    {
        Field = field;
        Order = order;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ValidationException("Parameter cannot be null or empty.");
        }

        if (trimmed[0] != '-')
        {
            result = new SortCriteria<T>(
                field: (T)Enum.Parse(typeof(T), trimmed, true),
                order: SortOrderType.Ascending
            );
        }
        else
        {
            result = new SortCriteria<T>(
                field: (T)Enum.Parse(typeof(T), trimmed[1..], true),
                order: SortOrderType.Descending
            );
        }

        return true;
    }
}