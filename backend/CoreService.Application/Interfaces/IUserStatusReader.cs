using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

/// <summary>
/// Anti-corruption port to the user bounded context. Authorization assignments require
/// an enabled human account; sanctions only require that the account exists.
/// </summary>
public interface IUserStatusReader
{
    ValueTask<bool> ExistsAsync(UserId userId, CancellationToken cancellationToken);
    ValueTask<bool> IsActiveAsync(UserId userId, CancellationToken cancellationToken);
}
