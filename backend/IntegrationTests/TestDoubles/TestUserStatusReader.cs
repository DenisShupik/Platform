using CoreService.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.TestDoubles;

public sealed class TestUserStatusReader(IEnumerable<UserId> activeUserIds) : IUserStatusReader
{
    private readonly HashSet<UserId> _existingUserIds = [.. activeUserIds];
    private readonly HashSet<UserId> _activeUserIds = [.. activeUserIds];

    public void SetActive(UserId userId, bool active)
    {
        _existingUserIds.Add(userId);
        if (active)
            _activeUserIds.Add(userId);
        else
            _activeUserIds.Remove(userId);
    }

    public void Remove(UserId userId)
    {
        _existingUserIds.Remove(userId);
        _activeUserIds.Remove(userId);
    }

    public ValueTask<bool> ExistsAsync(UserId userId, CancellationToken cancellationToken) =>
        ValueTask.FromResult(_existingUserIds.Contains(userId));

    public ValueTask<bool> IsActiveAsync(UserId userId, CancellationToken cancellationToken) =>
        ValueTask.FromResult(_activeUserIds.Contains(userId));
}
