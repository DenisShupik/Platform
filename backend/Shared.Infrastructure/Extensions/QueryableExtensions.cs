using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Extensions;

public static class QueryableExtensions
{
    private sealed class ScalarResult<T>
    {
        public required T Value { get; init; }
    }

    public static IQueryable<T> WithLockLinq2Db<T>(this IQueryable<T> queryable, LockMode lockMode) where T : notnull
    {
        return lockMode switch
        {
            LockMode.None => queryable,
            LockMode.ForShare => queryable.QueryHint(PostgreSQLHints.ForShare),
            LockMode.ForUpdate => queryable.QueryHint(PostgreSQLHints.ForUpdate),
            _ => throw new ArgumentOutOfRangeException(nameof(lockMode))
        };
    }

    public static IQueryable<TId> ToTvcLinqToDb<TId, TPrimitive>(
        this DbContext context,
        IdSet<TId, TPrimitive> values)
        where TId : struct, IId, IHasTryFrom<TId, TPrimitive>, IVogen<TId, TPrimitive>
        where TPrimitive : ISpanParsable<TPrimitive>
    {
        var primitiveValues = VogenValueObjectConversions.ToPrimitiveArray<TId, TPrimitive>(values);

        return context.Database
            .SqlQuery<ScalarResult<TPrimitive>>(
                $"SELECT value AS \"Value\" FROM UNNEST({primitiveValues}) AS source(value)")
            .Select(value => Sql.ConvertTo<TId>.From(value.Value));
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
}
