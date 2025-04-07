namespace SharedKernel.Application.Abstractions;

public sealed class LongIdList<T> : List<T> where T : IVogen<T, long>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out LongIdList<T>? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!long.TryParse(id, out var parsedId))
            {
                result = null;
                return false;
            }

            result.Add(T.From(parsedId));
        }

        return true;
    }
}