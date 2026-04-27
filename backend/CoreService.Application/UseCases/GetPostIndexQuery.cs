using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Index = Shared.Domain.ValueObjects.Index;

namespace CoreService.Application.UseCases;

using GetPostIndexQueryResult = Result<
    Index,
    PostNotFoundError,
    PermissionDeniedError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class
    GetPostIndexQuery : IQuery<GetPostIndexQueryResult>
{
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetPostIndexQueryHandler : IQueryHandler<GetPostIndexQuery, GetPostIndexQueryResult>
{
    private readonly IPostReadRepository _postReadRepository;

    public GetPostIndexQueryHandler(
        IPostReadRepository postReadRepository)
    {
        _postReadRepository = postReadRepository;
    }

    public Task<GetPostIndexQueryResult> HandleAsync(GetPostIndexQuery query, CancellationToken cancellationToken)
    {
        return _postReadRepository.GetPostIndexAsync(query, cancellationToken);
    }
}