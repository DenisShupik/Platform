using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Index = Shared.Domain.ValueObjects.Index;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    Task<Result<T, PostNotFoundError, PermissionDeniedError>> GetOneAsync<T>(GetPostQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    Task<Result<IReadOnlyList<T>, ThreadNotFoundError, PermissionDeniedError>> GetThreadPostsAsync<T>(
        GetThreadPostsPagedQuery<T> request, CancellationToken cancellationToken);

    Task<Result<Index, PostNotFoundError, PermissionDeniedError>> GetPostIndexAsync(GetPostIndexQuery query,
        CancellationToken cancellationToken);
}
