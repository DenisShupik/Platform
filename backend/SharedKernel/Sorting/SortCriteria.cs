using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Sorting;

public sealed class SortCriteria<T>
    where T : Enum
{
    public T Field { get; set; }
    public SortOrderType Order { get; set; }

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
                Order = SortOrderType.Ascending,
                // TODO: сделать Enum.TryParse
                Field = (T)Enum.Parse(typeof(T), trimmed, true),
            };
        }
        else
        {
            result = new SortCriteria<T>
            {
                Order = SortOrderType.Descending,
                // TODO: сделать Enum.TryParse
                Field = (T)Enum.Parse(typeof(T), trimmed[1..], true),
            };
        }

        return true;
    }
}