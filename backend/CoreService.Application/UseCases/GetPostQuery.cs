using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostQuery<T> : IQuery<Result<T, AccessPolicyViolationError, PolicyRestrictedError, PostNotFoundError>> where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetPostQueryHandler<T> : IQueryHandler<GetPostQuery<T>,
    Result<T, AccessPolicyViolationError, PolicyRestrictedError, PostNotFoundError>> where T : notnull
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IPostReadRepository _postReadRepository;

    public GetPostQueryHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IPostReadRepository postReadRepository
    )
    {
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
        _postReadRepository = postReadRepository;
    }

    public async Task<Result<T, AccessPolicyViolationError, PolicyRestrictedError, PostNotFoundError>> HandleAsync(
        GetPostQuery<T> query,
        CancellationToken cancellationToken)
    {
        var accessCheckResult = await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy,
            query.PostId,
            cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<T, PostNotFoundError>(out _, out var accessErrors))
            return accessErrors.Value;

        var postResult = await _postReadRepository.GetOneAsync<T>(query.PostId, cancellationToken);
        
        return postResult;
    }
}