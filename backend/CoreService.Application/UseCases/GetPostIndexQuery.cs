using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostIndexQuery : IQuery<OneOf<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetPostIndexQueryHandler : IQueryHandler<GetPostIndexQuery,
    OneOf<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>>
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

    public async Task<OneOf<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>> HandleAsync(
        GetPostIndexQuery query,
        CancellationToken cancellationToken)
    {
        var accessCheckResult = await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy,
            query.PostId,
            cancellationToken);

        if (!accessCheckResult.TryPickT0(out _, out var accessErrors))
            return accessErrors.Match<OneOf<PostIndex, AccessLevelError, AccessRestrictedError, PostNotFoundError>>(
                e1 => e1, e2 => e2);

        var postIndexResult = await _threadReadRepository.GetPostIndexAsync(query.PostId, cancellationToken);

        if (!postIndexResult.TryPickT0(out var postIndex, out var notFoundError)) return notFoundError;

        return postIndex;
    }
}