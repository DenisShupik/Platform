using Npgsql;

namespace IntegrationTests;

public sealed class DbContextConnectionStrings
{
    public required NpgsqlConnectionStringBuilder ReadonlyDbContext { get; init; }
    public required NpgsqlConnectionStringBuilder WritableDbContext { get; init; }
}