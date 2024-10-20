using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace NoteService.integrationTests;

[CollectionDefinition(nameof(IntegrationTestFactoryCollection))]
public class IntegrationTestFactoryCollection : ICollectionFixture<IntegrationTestFactory>
{
}

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("platform_db_tests")
        .WithUsername("admin")
        .WithPassword("12345678")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "NoteServiceOptions:ConnectionString",
            "Host=localhost;Port=5432;Database=platform_db_tests;Username=admin;Password=12345678;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;"
        );
    }
}