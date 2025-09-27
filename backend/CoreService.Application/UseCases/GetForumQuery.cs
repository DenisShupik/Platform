using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using OneOf;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class
    GetForumQuery<T> : IQuery<OneOf<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>
{
    public required UserId? QueriedBy { get; init; }
}



public sealed class GetForumQueryHandler<T> : IQueryHandler<GetForumQuery<T>,
    OneOf<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>
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

    public async Task<OneOf<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>> HandleAsync(
        GetForumQuery<T> query,
        CancellationToken cancellationToken)
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy, query.ForumId,
                cancellationToken);

        if (!accessCheckResult.TryPickT0(out _, out var accessErrors))
            return accessErrors.Match<OneOf<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>(
                e1 => e1, e2 => e2);

        var forumResult = await _repository.GetOneAsync<T>(query.ForumId, cancellationToken);

        return forumResult.Match<OneOf<T, ForumAccessLevelError, ForumAccessRestrictedError, ForumNotFoundError>>(
            forum => forum,
            notFound => notFound
        );
    }
}