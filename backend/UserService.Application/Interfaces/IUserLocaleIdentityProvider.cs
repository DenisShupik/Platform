using Shared.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserLocaleIdentityProvider
{
    Task<bool> ChangeLocaleAsync(
        UserId userId,
        Locale locale,
        CancellationToken cancellationToken);
}
