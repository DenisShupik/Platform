using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<Result<T, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>> GetOneAsync<T>(GetPostQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    public Task<Result<IReadOnlyList<T>, ThreadNotFoundError>> GetThreadPostsAsync<T>(
        GetThreadPostsPagedQuery<T> request, CancellationToken cancellationToken);

    public Task<Result<PostIndex, PostNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>
        GetPostIndexAsync(GetPostIndexQuery query, CancellationToken cancellationToken);
}