using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using NpgsqlTypes;

namespace CoreService.Infrastructure.Persistence.Extensions;

internal static class PostgreSqlFullTextSearch
{
    [Sql.Expression("websearch_to_tsquery('" + Constants.TextSearchConfiguration + "', {0})", ServerSideOnly = true)]
    public static NpgsqlTsQuery WebSearchToTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("to_tsquery('" + Constants.TextSearchConfiguration + "', {0} || ':*')", ServerSideOnly = true)]
    public static NpgsqlTsQuery PrefixToTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("{0} @@ {1}", IsPredicate = true, ServerSideOnly = true)]
    public static bool Matches(NpgsqlTsVector vector, NpgsqlTsQuery query) => throw new InvalidOperationException();

    [Sql.Expression("ts_rank({0}, {1})", ServerSideOnly = true)]
    public static float Rank(NpgsqlTsVector vector, NpgsqlTsQuery query) => throw new InvalidOperationException();

    [Sql.Expression("ts_headline('" + Constants.TextSearchConfiguration + "', {0}, {1}, 'StartSel=⟦, StopSel=⟧, MaxWords=35, MinWords=15, MaxFragments=2')", ServerSideOnly = true)]
    public static string Headline(PostContent document, NpgsqlTsQuery query) => throw new InvalidOperationException();
}
