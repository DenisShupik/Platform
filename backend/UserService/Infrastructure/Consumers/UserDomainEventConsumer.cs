using UserService.Domain.Events;
using UserService.Infrastructure.Cache;

namespace UserService.Infrastructure.Consumers;

public sealed class UserDomainEventConsumer
{
    private readonly IUserServiceCache _cache;

    public UserDomainEventConsumer(IUserServiceCache cache)
    {
        _cache = cache;
    }

    public async ValueTask ConsumeAsync(UserUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(domainEvent.UserId, cancellationToken);
    }
}