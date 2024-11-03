using LinqToDB;

namespace SharedKernel.Extensions;

public static class QueryableExtensions
{
    [Sql.Expression("DISTINCT ON({1}) {0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T1 SqlDistinctOn<T1, T2>([ExprParameter] this T1 input, T2 key)
    {
        throw new LinqToDBException($"{nameof(SqlDistinctOn)} server side only");
    }
}