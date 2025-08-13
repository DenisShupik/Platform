using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.SqlQuery;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain;
using SharedKernel.Domain.Interfaces;

public sealed class SqlValue<T>
{
    [LinqToDB.Mapping.Column(Name = "value")]
    public T Value { get; set; }
}

public static class QueryableExtensions
{
    [Sql.Expression("DISTINCT ON({1}) {0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T1 SqlDistinctOn<T1, T2>([ExprParameter] this T1 input, [ExprParameter] T2 key)
    {
        throw new LinqToDBException($"{nameof(SqlDistinctOn)} server side only");
    }

    [Sql.Expression("({0} IS NOT NULL)", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static bool SqlIsNotNull<T>([ExprParameter] this T? input)
    {
        throw new LinqToDBException($"{nameof(SqlIsNotNull)} server side only");
    }

    // [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    // public static T[] ToSqlArray<T>([ExprParameter] this Guid[] input)
    // {
    //     throw new LinqToDBException($"{nameof(ToSqlArray)} server side only");
    // }

    [Sql.Extension("{value} = ANY({array})", ServerSideOnly = true,
        IsNullable = Sql.IsNullableType.IfAnyParameterNullable, Precedence = Precedence.Comparison, IsPredicate = true)]
    public static bool ValueIsEqualToAny<TId, TUnderlying>(this IPostgreSQLExtensions? ext, [ExprParameter] TId value,
        [ExprParameter] TUnderlying[] array)
        where TId : IId, IVogen<TId, TUnderlying>
    {
        throw new ServerSideOnlyException(nameof(ValueIsEqualToAny));
    }

    [Sql.Expression("{0}", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static string ToSqlString<T>([ExprParameter] this T input)
    {
        throw new LinqToDBException($"{nameof(ToSqlString)} server side only");
    }

    public static IQueryable<T> ToTvcLinqToDb<T>(this DbContext context, T[] value)
    {
        return context.Database.SqlQuery<SqlValue<T>>($"SELECT * FROM UNNEST({value}) AS \"Value\"(value)")
            .Select(e => e.Value);
    }
}