using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Infrastructure.Diagnostics;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;

namespace IntegrationTests.Tests;

public sealed class RepositoryCallDiagnosticsTests
{
    [Test]
    public async Task RepositoryProxy_TagsLinqToDbCommandAcrossAsyncBoundary()
    {
        var contextAccessor = new RepositoryCallContextAccessor();
        var repository = RepositoryCallProxy<IRepositoryCallProbe>.Create(
            new RepositoryCallProbe(
                new LinqToDbRepositoryCallCommandInterceptor(contextAccessor)),
            contextAccessor);

        var linqToDbCommand = await repository.CaptureCommandAsync();
        const string expectedPrefix = "/* RepositoryCallProbe.CaptureCommandAsync */";

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
            descriptor.ServiceType == typeof(EfRepositoryCallQueryInterceptor) ||
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
            descriptor.ServiceType == typeof(EfRepositoryCallQueryInterceptor) ||
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

    [Test]
    public async Task RepositoryProxy_TagsEveryAsyncReturnShape()
    {
        var contextAccessor = new RepositoryCallContextAccessor();
        var target = new AsyncReturnRepositoryCallProbe(contextAccessor);
        var repository = RepositoryCallProxy<IAsyncReturnRepositoryCallProbe>.Create(target, contextAccessor);

        await repository.CaptureTaskAsync();
        var taskResult = await repository.CaptureTaskOfTAsync();
        await repository.CaptureValueTaskAsync();
        var valueTaskResult = await repository.CaptureValueTaskOfTAsync();

        await Assert.That(target.TaskCommand)
            .StartsWith("/* AsyncReturnRepositoryCallProbe.CaptureTaskAsync */");
        await Assert.That(taskResult)
            .StartsWith("/* AsyncReturnRepositoryCallProbe.CaptureTaskOfTAsync */");
        await Assert.That(target.ValueTaskCommand)
            .StartsWith("/* AsyncReturnRepositoryCallProbe.CaptureValueTaskAsync */");
        await Assert.That(valueTaskResult)
            .StartsWith("/* AsyncReturnRepositoryCallProbe.CaptureValueTaskOfTAsync */");
    }

    [Test]
    public async Task DisabledRepositoryDiagnostics_DisablesSensitiveSqlLogging()
    {
        var services = new ServiceCollection();
        services.Configure<TestDbOptions>(options =>
        {
            const string connectionString = "Host=localhost;Database=test;Username=test;Password=test";
            options.ReadonlyConnectionString = connectionString;
            options.WritableConnectionString = connectionString;
        });
        services.RegisterDbContexts<TestReadDbContext, TestWriteDbContext, TestDbOptions>(
            "test",
            enableRepositoryCallDiagnostics: false);

        await using var serviceProvider = services.BuildServiceProvider();
        var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<TestReadDbContext>>();
        var coreOptions = dbContextOptions.Extensions.OfType<CoreOptionsExtension>().Single();
        var loggingOptions = serviceProvider.GetRequiredService<IOptions<LoggerFilterOptions>>().Value;

        await Assert.That(coreOptions.IsSensitiveDataLoggingEnabled).IsFalse();
        await Assert.That(loggingOptions.Rules.Any(rule =>
            rule.CategoryName == DbLoggerCategory.Database.Command.Name &&
            rule.LogLevel == LogLevel.Warning)).IsTrue();
        await Assert.That(loggingOptions.Rules.Any(rule =>
            rule.CategoryName == "LinqToDB" &&
            rule.LogLevel == LogLevel.Warning)).IsTrue();
    }

    public interface IRepositoryCallProbe
    {
        Task<string> CaptureCommandAsync();
    }

    private sealed class RepositoryCallProbe(
        LinqToDbRepositoryCallCommandInterceptor linqToDbInterceptor) : IRepositoryCallProbe
    {
        public async Task<string> CaptureCommandAsync()
        {
            await Task.Yield();

            using var linqToDbCommand = new NpgsqlCommand("SELECT 2");
            linqToDbInterceptor.CommandInitialized(default, linqToDbCommand);

            return linqToDbCommand.CommandText;
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

    public interface IAsyncReturnRepositoryCallProbe
    {
        Task CaptureTaskAsync();
        Task<string> CaptureTaskOfTAsync();
        ValueTask CaptureValueTaskAsync();
        ValueTask<string> CaptureValueTaskOfTAsync();
    }

    private sealed class AsyncReturnRepositoryCallProbe(RepositoryCallContextAccessor contextAccessor)
        : IAsyncReturnRepositoryCallProbe
    {
        public string TaskCommand { get; private set; } = string.Empty;
        public string ValueTaskCommand { get; private set; } = string.Empty;

        public async Task CaptureTaskAsync()
        {
            await Task.Yield();
            TaskCommand = CaptureCommand();
        }

        public async Task<string> CaptureTaskOfTAsync()
        {
            await Task.Yield();
            return CaptureCommand();
        }

        public async ValueTask CaptureValueTaskAsync()
        {
            await Task.Yield();
            ValueTaskCommand = CaptureCommand();
        }

        public async ValueTask<string> CaptureValueTaskOfTAsync()
        {
            await Task.Yield();
            return CaptureCommand();
        }

        private string CaptureCommand()
        {
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
