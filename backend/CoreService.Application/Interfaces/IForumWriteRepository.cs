using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IForumWriteRepository
{
    public Task<Result<T, ForumNotFoundError>> GetAsync<T>(ForumId forumId, CancellationToken cancellationToken)
        where T : class, IHasForumId;

    public Task AddAsync(Forum forum, CancellationToken cancellationToken);
}