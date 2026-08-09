using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Infrastructure.Diagnostics;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;

namespace IntegrationTests.Tests;

public sealed class RepositoryCallDiagnosticsTests
{
    [Test]
    public async Task RepositoryProxy_TagsEfCoreAndLinqToDbCommandsAcrossAsyncBoundary()
    {
        var contextAccessor = new RepositoryCallContextAccessor();
        var repository = RepositoryCallProxy<IRepositoryCallProbe>.Create(
            new RepositoryCallProbe(
                new EfRepositoryCallCommandInterceptor(contextAccessor),
                new LinqToDbRepositoryCallCommandInterceptor(contextAccessor)),
            contextAccessor);

        var (efCoreCommand, linqToDbCommand) = await repository.CaptureCommandsAsync();
        const string expectedPrefix = "/* RepositoryCallProbe.CaptureCommandsAsync */";

        await Assert.That(efCoreCommand).StartsWith(expectedPrefix);
        await Assert.That(linqToDbCommand).StartsWith(expectedPrefix);
    }

    [Test]
    public async Task DisabledRepositoryDiagnostics_DoesNotRegisterInterceptorsOrProxyRepository()
    {
        var services = new ServiceCollection();
        services.RegisterDbContexts<TestReadDbContext, TestWriteDbContext, TestDbOptions>(
            "test",
            enableRepositoryCallDiagnostics: false);
        services.AddRepository<INoOpRepository, NoOpRepository>();

        var repositoryDescriptor = services.Single(descriptor =>
            descriptor.ServiceType == typeof(INoOpRepository));

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<INoOpRepository>();

        await Assert.That(repository.GetType()).IsEqualTo(typeof(NoOpRepository));
        await Assert.That(repositoryDescriptor.ImplementationType).IsEqualTo(typeof(NoOpRepository));
        await Assert.That(repositoryDescriptor.ImplementationFactory).IsNull();
        await Assert.That(services.Any(descriptor =>
            descriptor.ServiceType == typeof(EfRepositoryCallCommandInterceptor) ||
            descriptor.ServiceType == typeof(LinqToDbRepositoryCallCommandInterceptor))).IsFalse();
    }

    [Test]
    public async Task RepositoryRegistration_CanOptOutOfEnabledDiagnostics()
    {
        var services = new ServiceCollection();
        services.RegisterDbContexts<TestReadDbContext, TestWriteDbContext, TestDbOptions>(
            "test",
            enableRepositoryCallDiagnostics: true);
        services.AddRepository<INoOpRepository, NoOpRepository>(enableCallDiagnostics: false);

        var repositoryDescriptor = services.Single(descriptor =>
            descriptor.ServiceType == typeof(INoOpRepository));

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<INoOpRepository>();

        await Assert.That(repository.GetType()).IsEqualTo(typeof(NoOpRepository));
        await Assert.That(repositoryDescriptor.ImplementationType).IsEqualTo(typeof(NoOpRepository));
        await Assert.That(repositoryDescriptor.ImplementationFactory).IsNull();
        await Assert.That(services.Any(descriptor =>
            descriptor.ServiceType == typeof(EfRepositoryCallCommandInterceptor) ||
            descriptor.ServiceType == typeof(LinqToDbRepositoryCallCommandInterceptor))).IsTrue();
    }

    [Test]
    public async Task RepositoryProxy_IsolatesConcurrentAsyncRepositoryCalls()
    {
        var contextAccessor = new RepositoryCallContextAccessor();
        var repository = RepositoryCallProxy<IConcurrentRepositoryCallProbe>.Create(
            new ConcurrentRepositoryCallProbe(contextAccessor),
            contextAccessor);

        var first = repository.CaptureFirstAsync();
        var second = repository.CaptureSecondAsync();
        var commands = await Task.WhenAll(first, second);

        await Assert.That(commands[0]).StartsWith("/* ConcurrentRepositoryCallProbe.CaptureFirstAsync */");
        await Assert.That(commands[1]).StartsWith("/* ConcurrentRepositoryCallProbe.CaptureSecondAsync */");
    }

    public interface IRepositoryCallProbe
    {
        Task<(string EfCoreCommand, string LinqToDbCommand)> CaptureCommandsAsync();
    }

    private sealed class RepositoryCallProbe(
        EfRepositoryCallCommandInterceptor efCoreInterceptor,
        LinqToDbRepositoryCallCommandInterceptor linqToDbInterceptor) : IRepositoryCallProbe
    {
        public async Task<(string EfCoreCommand, string LinqToDbCommand)> CaptureCommandsAsync()
        {
            await Task.Yield();

            using var efCoreCommand = new NpgsqlCommand("SELECT 1");
            efCoreInterceptor.CommandInitialized(null!, efCoreCommand);

            using var linqToDbCommand = new NpgsqlCommand("SELECT 2");
            linqToDbInterceptor.CommandInitialized(default, linqToDbCommand);

            return (efCoreCommand.CommandText, linqToDbCommand.CommandText);
        }
    }

    public interface IConcurrentRepositoryCallProbe
    {
        Task<string> CaptureFirstAsync();
        Task<string> CaptureSecondAsync();
    }

    private sealed class ConcurrentRepositoryCallProbe(RepositoryCallContextAccessor contextAccessor)
        : IConcurrentRepositoryCallProbe
    {
        public Task<string> CaptureFirstAsync() => CaptureCommandAsync();

        public Task<string> CaptureSecondAsync() => CaptureCommandAsync();

        private async Task<string> CaptureCommandAsync()
        {
            await Task.Yield();
            using var command = new NpgsqlCommand("SELECT 1");
            return RepositoryCallCommandTagger.AddRepositoryCall(command, contextAccessor).CommandText;
        }
    }

    public interface INoOpRepository;

    private sealed class NoOpRepository : INoOpRepository;

    private sealed class TestReadDbContext(DbContextOptions<TestReadDbContext> options)
        : DbContext(options), IReadDbContext;

    private sealed class TestWriteDbContext(DbContextOptions<TestWriteDbContext> options)
        : DbContext(options), IWriteDbContext;

    private sealed class TestDbOptions : IDbOptions
    {
        public string ReadonlyConnectionString { get; set; } = string.Empty;
        public string WritableConnectionString { get; set; } = string.Empty;
    }
}
