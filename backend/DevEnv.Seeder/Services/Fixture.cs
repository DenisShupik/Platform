using System.Collections.Immutable;

namespace DevEnv.Seeder.Services;

public sealed class Fixture
{
    private const int UserCount = 10;

    public readonly ImmutableArray<string> Users;

    public string GetRandomUser() => Users[Random.Shared.Next(0, UserCount)];

    public Fixture()
    {
        Users = [..Enumerable.Range(1, UserCount).Select(x => $"user{x}").ToArray()];
    }
}