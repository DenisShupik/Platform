using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using OneOf;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class GetPostQuery<T> : IQuery<OneOf<T, PostNotFoundError>>;


public sealed class GetPostQueryHandler<T> : IQueryHandler<GetPostQuery<T>, OneOf<T, PostNotFoundError>>
{
    private readonly IPostReadRepository _repository;

    public GetPostQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<T, PostNotFoundError>> HandleAsync(GetPostQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetOneAsync<T>(query.PostId, cancellationToken);
    }
}