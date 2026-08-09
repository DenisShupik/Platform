using System.Collections.Concurrent;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace IntegrationTests.Tests;

public sealed class RepositoryCallDiagnosticsIntegrationTests
{
    [ClassDataSource<NotificationServiceTestsFixture<RepositoryCallDiagnosticsIntegrationTests>>(
        Shared = SharedType.PerClass)]
    public required NotificationServiceTestsFixture<RepositoryCallDiagnosticsIntegrationTests> Fixture { get; init; }

    [Test]
    public async Task EfCoreRepositoryQuery_IsTaggedInExecutedCommand(CancellationToken cancellationToken)
    {
        var loggerProvider = new CommandCaptureLoggerProvider();
        Fixture.Services.GetRequiredService<ILoggerFactory>().AddProvider(loggerProvider);

        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();

        await repository.ExistsAsync(
            Fixture.TestUserId,
            ThreadId.From(Guid.NewGuid()),
            cancellationToken);

        await Assert.That(loggerProvider.Messages.Any(message =>
                message.Contains("ThreadSubscriptionReadRepository.ExistsAsync", StringComparison.Ordinal) &&
                message.Contains("SELECT EXISTS", StringComparison.Ordinal)))
            .IsTrue();
    }

    private sealed class CommandCaptureLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) =>
            new CommandCaptureLogger(categoryName, Messages);

        public void Dispose()
        {
        }
    }

    private sealed class CommandCaptureLogger(
        string categoryName,
        ConcurrentQueue<string> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) =>
            categoryName == DbLoggerCategory.Database.Command.Name && logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel)) messages.Enqueue(formatter(state, exception));
        }
    }
}
