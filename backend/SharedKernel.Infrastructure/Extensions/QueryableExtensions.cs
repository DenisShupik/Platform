using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.SqlQuery;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Enums;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public sealed class SqlValue<T>
    {
        [LinqToDB.Mapping.Column(Name = "value")]
        public required T Value { get; init; }
    }

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

    [Sql.Expression("{0} NULLS LAST", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T SqlNullsLast<T>([ExprParameter] this T input)
    {
        throw new LinqToDBException($"{nameof(SqlNullsLast)} server side only");
    }

    [Sql.Expression("{0} DESC NULLS LAST", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T SqlDescNullsLast<T>([ExprParameter] this T input)
    {
        throw new LinqToDBException($"{nameof(SqlDescNullsLast)} server side only");
    }

    public static IQueryable<T> ToTvcLinqToDb<T>(this DbContext context, T[] value)
    {
        return context.Database.SqlQuery<SqlValue<T>>($"SELECT * FROM UNNEST({value}) AS \"Value\"(value)")
            .Select(e => e.Value);
    }

    public static IOrderedQueryable<T> ApplySort<T, TKey>(this IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector, SortOrderType sortOrder, bool isFirst)
    {
        var ascending = sortOrder == SortOrderType.Ascending;
        if (isFirst)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        var ordered = (IOrderedQueryable<T>)source;
        return ascending
            ? ordered.ThenBy(keySelector)
            : ordered.ThenByDescending(keySelector);
    }

    public static IQueryable<T> ApplyPagination<T, TPaginationLimit>(this IQueryable<T> queryable,
        IHasPagination<TPaginationLimit> request)
        where TPaginationLimit : struct, IPaginationLimit, IVogen<TPaginationLimit, int>
    {
        if (request.Offset is { Value: not 0 } offset) queryable = queryable.Skip(offset.Value);
        return queryable.Take(request.Limit?.Value ?? TPaginationLimit.Default);
    }
}