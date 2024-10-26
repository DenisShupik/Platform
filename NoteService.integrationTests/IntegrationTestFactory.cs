using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace NoteService.integrationTests;

[CollectionDefinition(nameof(IntegrationTestFactoryCollection))]
public class IntegrationTestFactoryCollection : ICollectionFixture<IntegrationTestFactory>
{
}

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly Note Note1;

    private readonly IConfiguration _configuration;
    private readonly PostgreSqlContainer _container;

    public IntegrationTestFactory()
    {
        Note1 = new Note
        {
            NoteId = 1,
            UserId = Guid.Parse("B54068C4-A920-4685-A2AD-08C22B8E6946"),
            Title = "Title"
        };

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
            .Build();

        var options =
            new NpgsqlConnectionStringBuilder(_configuration.GetSection("NoteServiceOptions:ConnectionString").Value);

        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase(options.Database)
            .WithUsername(options.Username)
            .WithPassword(options.Password)
            .WithPortBinding(options.Port, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        using var scope = Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Notes.Add(Note1);
        await dbContext.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddConfiguration(_configuration);
        });
    }
}