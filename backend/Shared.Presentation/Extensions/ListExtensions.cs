namespace Shared.Presentation.Extensions;

public static class ListExtensions
{
    public static IList<T> AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        foreach (var item in items) list.Add(item);
        return list;
    }
}