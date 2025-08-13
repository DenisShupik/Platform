using System.Diagnostics.CodeAnalysis;

namespace SharedKernel.Application.Abstractions;

public sealed class HashSetEnum<T> : HashSet<T> where T : struct, Enum
{
    public static bool TryParse(string? value, IFormatProvider? provider,
        [NotNullWhen(true)] out HashSetEnum<T>? result)
    {
        result = [];

        if (string.IsNullOrEmpty(value))
            return true;

        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!Enum.TryParse<T>(item, true, out var parsed))
            {
                result = null;
                return false;
            }

            result.Add(parsed);
        }

        return true;
    }
}