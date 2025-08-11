using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Application.Abstractions;

public sealed class IdSet<T> : HashSet<T> where T : IId, IParsable<T>
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

        if (string.IsNullOrEmpty(value)) return false;

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0) return false;

        foreach (var id in parts)
        {
            if (!T.TryParse(id, provider, out var parsed))
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