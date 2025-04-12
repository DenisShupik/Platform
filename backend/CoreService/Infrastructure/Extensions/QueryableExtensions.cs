using LinqToDB;

namespace CoreService.Infrastructure.Extensions;

public static class QueryableExtensions
{
    [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T2[] ToSqlGuid<T1, T2>([ExprParameter] this T1[] input)
    {
        throw new LinqToDBException($"{nameof(ToSqlGuid)} server side only");
    }

    [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static string VogenToSql<T>([ExprParameter] this T input)
    {
        throw new LinqToDBException($"{nameof(VogenToSql)} server side only");
    }
}