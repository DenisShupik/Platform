namespace SharedKernel.Application.Abstractions;

public sealed class GuidIdList<T> : List<T> where T : IVogen<T, Guid>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out GuidIdList<T>? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!Guid.TryParse(id, out var parsedId))
            {
                result = null;
                return false;
            }

            result.Add(T.From(parsedId));
        }

        return true;
    }
}