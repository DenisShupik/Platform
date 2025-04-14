using LinqToDB;

namespace CoreService.Infrastructure.Extensions;

public static class QueryableExtensions
{
    [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T[] ToSqlArray<T>([ExprParameter] this Guid[] input)
    {
        throw new LinqToDBException($"{nameof(ToSqlArray)} server side only");
    }

    [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static string ToSqlString<T>([ExprParameter] this T input)
    {
        throw new LinqToDBException($"{nameof(ToSqlString)} server side only");
    }
}