using NotificationService.Application.Dtos;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IUserDirectoryReader
{
    ValueTask<IReadOnlyList<UserSummaryDto>> GetUsersAsync(
        ICollection<UserId> userIds,
        CancellationToken cancellationToken);
}
