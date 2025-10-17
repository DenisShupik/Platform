using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class
    GetForumQuery<T> : IQuery<Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetForumQueryHandler<T> : IQueryHandler<GetForumQuery<T>,
    Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>>
    where T : notnull
{
    private readonly IForumReadRepository _repository;

    public GetForumQueryHandler(
        IForumReadRepository repository
    )
    {
        _repository = repository;
    }

    public Task<Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>> HandleAsync(
        GetForumQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetOneAsync(query, cancellationToken);
    }
}