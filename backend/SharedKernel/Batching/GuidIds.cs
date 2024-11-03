namespace SharedKernel.Batching;

public sealed class GuidIds : List<Guid>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out GuidIds? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!Guid.TryParse(id, out var intId))
            {
                result = null;
                return false;
            }

            result.Add(intId);
        }

        return true;
    }
}