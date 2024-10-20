using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NoteService.integrationTests;

[CollectionDefinition(nameof(IntegrationTestFactoryCollection))]
public class IntegrationTestFactoryCollection : ICollectionFixture<IntegrationTestFactory>
{
}

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly Note Note1;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("platform_db_tests")
        .WithUsername("admin")
        .WithPassword("12345678")
        .WithPortBinding(5432,5432)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public IntegrationTestFactory()
    {
        Note1 = new Note
        {
            NoteId = 1,
            UserId = Guid.Parse("B54068C4-A920-4685-A2AD-08C22B8E6946"),
            Title = "Title"
        };
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var dbContextFactory = scopedServices.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
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
        builder.UseSetting(
            "NoteServiceOptions:ConnectionString",
            "Host=localhost;Port=5432;Database=platform_db_tests;Username=admin;Password=12345678;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;"
        );
    }
}