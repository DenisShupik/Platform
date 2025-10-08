using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using GetPostIndexQueryResult = Result<
    PostIndex,
    PostNotFoundError,
    PolicyViolationError,
    AccessPolicyRestrictedError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostIndexQuery : IQuery<GetPostIndexQueryResult>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetPostIndexQueryHandler : IQueryHandler<GetPostIndexQuery, GetPostIndexQueryResult>
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IPostReadRepository _postReadRepository;

    public GetPostIndexQueryHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IPostReadRepository postReadRepository)
    {
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
        _postReadRepository = postReadRepository;
    }

    public Task<GetPostIndexQueryResult> HandleAsync(GetPostIndexQuery query, CancellationToken cancellationToken)
    {
        return _postReadRepository.GetPostIndexAsync(query, cancellationToken);
    }
}