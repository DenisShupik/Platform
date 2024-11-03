namespace SharedKernel.Batching;

public sealed class LongIds : List<long>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out LongIds? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!int.TryParse(id, out var intId))
            {
                result = null;
                return false;
            }

            result.Add(intId);
        }

        return true;
    }
}