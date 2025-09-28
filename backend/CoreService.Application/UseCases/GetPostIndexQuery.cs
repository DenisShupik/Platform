using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostIndexQuery : IQuery<Result<PostIndex, AccessLevelError, AccessRestrictedError,
    PostNotFoundError>>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetPostIndexQueryHandler : IQueryHandler<GetPostIndexQuery,
    Result<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>>
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IThreadReadRepository _threadReadRepository;

    public GetPostIndexQueryHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IThreadReadRepository threadReadRepository)
    {
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
        _threadReadRepository = threadReadRepository;
    }

    public async Task<Result<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>> HandleAsync(
        GetPostIndexQuery query, CancellationToken cancellationToken)
    {
        var accessCheckResult = await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy,
            query.PostId,
            cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<PostIndex, PostNotFoundError>(out _, out var accessErrors))
            return accessErrors.Value;

        var postIndexResult = await _threadReadRepository.GetPostIndexAsync(query.PostId, cancellationToken);

        return postIndexResult;
    }
}