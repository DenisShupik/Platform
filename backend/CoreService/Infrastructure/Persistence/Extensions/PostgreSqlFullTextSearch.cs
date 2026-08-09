using LinqToDB;
using NpgsqlTypes;

namespace CoreService.Infrastructure.Persistence.Extensions;

internal static class PostgreSqlFullTextSearch
{
    [Sql.Expression("websearch_to_tsquery('" + Constants.EnglishTextSearchConfiguration + "', {0})", ServerSideOnly = true)]
    public static NpgsqlTsQuery WebSearchToEnglishTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("websearch_to_tsquery('" + Constants.RussianTextSearchConfiguration + "', {0})", ServerSideOnly = true)]
    public static NpgsqlTsQuery WebSearchToRussianTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("to_tsquery('" + Constants.EnglishTextSearchConfiguration + "', {0} || ':*')", ServerSideOnly = true)]
    public static NpgsqlTsQuery PrefixToEnglishTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("to_tsquery('" + Constants.RussianTextSearchConfiguration + "', {0} || ':*')", ServerSideOnly = true)]
    public static NpgsqlTsQuery PrefixToRussianTsQuery(string term) => throw new InvalidOperationException();

    [Sql.Expression("{0} @@ {1}", IsPredicate = true, ServerSideOnly = true)]
    public static bool Matches(NpgsqlTsVector vector, NpgsqlTsQuery query) => throw new InvalidOperationException();

    [Sql.Expression("ts_rank({0}, {1})", ServerSideOnly = true)]
    public static float Rank(NpgsqlTsVector vector, NpgsqlTsQuery query) => throw new InvalidOperationException();

    [Sql.Expression("ts_headline('" + Constants.EnglishTextSearchConfiguration + "', {0}, {1}, 'StartSel=⟦, StopSel=⟧, MaxWords=35, MinWords=15, MaxFragments=2')", ServerSideOnly = true)]
    public static string EnglishHeadline(string document, NpgsqlTsQuery query) => throw new InvalidOperationException();

    [Sql.Expression("ts_headline('" + Constants.RussianTextSearchConfiguration + "', {0}, {1}, 'StartSel=⟦, StopSel=⟧, MaxWords=35, MinWords=15, MaxFragments=2')", ServerSideOnly = true)]
    public static string RussianHeadline(string document, NpgsqlTsQuery query) => throw new InvalidOperationException();
}
