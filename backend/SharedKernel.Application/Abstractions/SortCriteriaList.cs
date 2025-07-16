namespace SharedKernel.Application.Abstractions;

public sealed class SortCriteriaList<T> : List<SortCriteria<T>> where T : Enum
{
    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteriaList<T>? result)
    {
        result = [];

        if (string.IsNullOrEmpty(value))
            return true;

        foreach (var id in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!SortCriteria<T>.TryParse(id, provider, out var parsed))
            {
                result = null;
                return false;
            }

            result.Add(parsed);
        }

        return true;
    }
}