using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IForumRepository
{
    public Task<OneOf<T, ForumNotFoundError>> GetAsync<T>(ForumId forumId, CancellationToken cancellationToken)
        where T : class, IHasForumId;

    public Task AddAsync(Forum forum, CancellationToken cancellationToken);
}