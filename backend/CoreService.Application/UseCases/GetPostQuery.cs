using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class GetPostQuery<T> : IQuery<Result<T, PostNotFoundError, PermissionDeniedError>>
    where T : notnull
{
    public required ActorContext? QueriedBy { get; init; }
}

public sealed class
    GetPostQueryHandler<T> : IQueryHandler<GetPostQuery<T>, Result<T, PostNotFoundError, PermissionDeniedError>>
    where T : notnull
{
    private readonly IPostReadRepository _postReadRepository;

    public GetPostQueryHandler(IPostReadRepository postReadRepository)
    {
        _postReadRepository = postReadRepository;
    }

    public Task<Result<T, PostNotFoundError, PermissionDeniedError>> HandleAsync(GetPostQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _postReadRepository.GetOneAsync(query, cancellationToken);
    }
}
