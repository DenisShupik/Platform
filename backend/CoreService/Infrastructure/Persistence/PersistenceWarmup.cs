using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;

namespace CoreService.Infrastructure.Persistence;

public static class PersistenceWarmup
{
    private static readonly ThreadId NonexistentThreadId =
        ThreadId.From(Guid.CreateVersion7(DateTimeOffset.UnixEpoch));

    public static async Task WarmUpPersistenceAsync(this WebApplication application)
    {
        await using var scope = application.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IThreadWriteRepository>();

        _ = await repository.GetOneAsync(
            NonexistentThreadId,
            LockMode.ForUpdate,
            application.Lifetime.ApplicationStopping);
    }
}
