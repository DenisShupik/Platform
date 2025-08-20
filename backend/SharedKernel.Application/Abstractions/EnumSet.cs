namespace SharedKernel.Application.Abstractions;


public sealed class EnumSet<T> : HashSet<T> where T : struct, Enum
{
    private EnumSet()
    {
    }

    private EnumSet(IEnumerable<T> collection) : base(collection)
    {
    }

    public static EnumSet<T> Create(IEnumerable<T> collection)
    {
        var result = new EnumSet<T>(collection);
        return result.Count == 0 ? throw new ArgumentException(nameof(collection)) : result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out EnumSet<T>? result)
    {
        result = null;

        if (string.IsNullOrEmpty(value)) return false;

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0) return false;

        foreach (var id in parts)
        {
            if (!Enum.TryParse<T>(id, true, out var parsed))
            {
                result = null;
                return false;
            }

            (result ??= []).Add(parsed);
        }

        if (result?.Count != 0) return true;

        result = null;
        return false;
    }
}