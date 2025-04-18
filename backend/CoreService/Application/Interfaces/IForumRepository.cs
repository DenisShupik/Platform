using CoreService.Domain.Entities;

namespace CoreService.Application.Interfaces;

public interface IForumRepository
{
    public Task AddAsync(Forum forum, CancellationToken cancellationToken);
}