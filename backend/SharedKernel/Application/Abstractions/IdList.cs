using SharedKernel.Domain;

namespace SharedKernel.Application.Abstractions;

public sealed class IdList<T> : List<T> where T : IId, IParsable<T>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out IdList<T>? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!T.TryParse(id, provider, out var parsed))
            {
                result = null;
                return false;
            }

            result.Add(parsed);
        }

        return true;
    }
}