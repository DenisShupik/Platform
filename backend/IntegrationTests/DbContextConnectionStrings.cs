using Npgsql;

namespace IntegrationTests;

public sealed class DbContextConnectionStrings
{
    public required NpgsqlConnectionStringBuilder ReadDbContext { get; init; }
    public required NpgsqlConnectionStringBuilder WriteDbContext { get; init; }
}