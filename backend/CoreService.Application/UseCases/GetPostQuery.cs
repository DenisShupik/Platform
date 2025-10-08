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
    GetPostQuery<T> : IQuery<Result<T, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetPostQueryHandler<T> : IQueryHandler<GetPostQuery<T>,
    Result<T, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>> where T : notnull
{
    private readonly IPostReadRepository _postReadRepository;

    public GetPostQueryHandler(
        IPostReadRepository postReadRepository
    )
    {
        _postReadRepository = postReadRepository;
    }

    public Task<Result<T, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>> HandleAsync(
        GetPostQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _postReadRepository.GetOneAsync(query, cancellationToken);
    }
}