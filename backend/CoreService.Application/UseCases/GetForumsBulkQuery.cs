using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class
    GetForumsBulkQuery<T> : IQuery<
    Dictionary<ForumId, Result<T, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>>
    where T : notnull
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId, Guid> ForumIds { get; init; }

    public required UserId? QueriedBy { get; init; }
}

public sealed class GetForumsBulkQueryHandler<T> : IQueryHandler<GetForumsBulkQuery<T>,
    Dictionary<ForumId, Result<T, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>>
    where T : notnull
{
    private readonly IForumReadRepository _repository;

    public GetForumsBulkQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<ForumId, Result<T, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>>
        HandleAsync(GetForumsBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync(query, cancellationToken);
}