using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class
    GetForumQuery<T> : IQuery<Result<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetForumQueryHandler<T> : IQueryHandler<GetForumQuery<T>,
    Result<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>
    where T : notnull
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IForumReadRepository _repository;

    public GetForumQueryHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IForumReadRepository repository
    )
    {
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
        _repository = repository;
    }

    public async Task<Result<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>> HandleAsync(
        GetForumQuery<T> query,
        CancellationToken cancellationToken)
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy, query.ForumId,
                cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<T, ForumNotFoundError>(out _, out var accessErrors))
            return accessErrors.Value;

        var forumResult = await _repository.GetOneAsync<T>(query.ForumId, cancellationToken);

        return forumResult;
    }
}