using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using OneOf;
using OneOf.Types;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostQuery<T> : IQuery<OneOf<T, AccessLevelError, AccessRestrictedError, PostNotFoundError>>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetPostQueryHandler<T> : IQueryHandler<GetPostQuery<T>,
    OneOf<T, AccessLevelError, AccessRestrictedError, PostNotFoundError>>
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

    public async Task<OneOf<T, AccessLevelError, AccessRestrictedError, PostNotFoundError>> HandleAsync(
        GetPostQuery<T> query,
        CancellationToken cancellationToken)
    {
        var accessCheckResult = await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy,
            query.PostId,
            cancellationToken);

        if (!accessCheckResult.TryPickT0(out _, out var accessErrors))
            return accessErrors.Match<OneOf<T, AccessLevelError, AccessRestrictedError, PostNotFoundError>>(
                e1 => e1, e2 => e2);

        var postResult = await _postReadRepository.GetOneAsync<T>(query.PostId, cancellationToken);

        if (!postResult.TryPickT0(out var post, out var notFoundError)) return notFoundError;

        return post;
    }
}