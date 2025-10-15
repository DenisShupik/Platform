using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.SqlQuery;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Extensions;

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
        throw new ServerSideOnlyException(nameof(SqlDistinctOn));
    }

    [Sql.Expression("{0} IS NOT NULL", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static bool SqlIsNotNull<T>([ExprParameter] this T? input)
    {
        throw new ServerSideOnlyException(nameof(SqlIsNotNull));
    }
    
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
        throw new ServerSideOnlyException(nameof(ToSqlString));
    }

    [Sql.Expression("{0} NULLS LAST", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T SqlNullsLast<T>([ExprParameter] this T input)
    {
        throw new ServerSideOnlyException(nameof(SqlNullsLast));
    }

    [Sql.Expression("{0} DESC NULLS LAST", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T SqlDescNullsLast<T>([ExprParameter] this T input)
    {
        throw new ServerSideOnlyException(nameof(SqlDescNullsLast));
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

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, IHasPagination request)
    {
        if (request.Offset != 0) queryable = queryable.Skip(request.Offset.Value);
        return queryable.Take(request.Limit.Value);
    }

    [ExpressionMethod(nameof(UnnestImpl))]
    public static IQueryable<T> Unnest<T>(this IDataContext dataContext, EnumSet<T> enumSet) where T : struct, Enum
        => throw new ServerSideOnlyException(nameof(Unnest));

    static Expression<Func<IDataContext, EnumSet<T>, IQueryable<T>>> UnnestImpl<T>() where T : struct, Enum =>
        (dataContext, enumSet) => dataContext.FromSqlScalar<T>($"UNNEST({enumSet})");

    [Sql.Function(Name = "GREATEST", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static T Greatest<T>(this IPostgreSQLExtensions? ext, params T[] input)
    {
        throw new ServerSideOnlyException(nameof(Greatest));
    }

    [Sql.Extension("ARRAY[{values, ','}]", IgnoreGenericParameters = true, ServerSideOnly = true)]
    public static T[] SqlArray<T>([ExprParameter] params T[] values)
    {
        throw new ServerSideOnlyException(nameof(SqlArray));
    }
}