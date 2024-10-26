using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TopicService.IntegrationTests;

[CollectionDefinition(nameof(IntegrationTestFactoryCollection))]
public class IntegrationTestFactoryCollection : ICollectionFixture<IntegrationTestFactory>
{
}

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{ 
    private readonly IConfiguration _configuration;
    private readonly PostgreSqlContainer _container;

    public IntegrationTestFactory()
    {
        
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
            .Build();

        var options =
            new NpgsqlConnectionStringBuilder(_configuration.GetSection("TopicServiceOptions:ConnectionString").Value);

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
        // using var scope = Services.CreateScope();
        // var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        // await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        // dbContext.Notes.Add(ForumThread1);
        // await dbContext.SaveChangesAsync();
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