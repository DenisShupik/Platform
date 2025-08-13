using UserService.Domain.DomainEvents;
using UserService.Infrastructure.Cache;

namespace UserService.Infrastructure.Consumers;

public sealed class UserDomainEventConsumer
{
    private readonly IUserServiceCache _cache;

    public UserDomainEventConsumer(IUserServiceCache cache)
    {
        _cache = cache;
    }

    public async ValueTask ConsumeAsync(UserUpdatedDomainEvent @event, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(@event.UserId, cancellationToken);
    }
}