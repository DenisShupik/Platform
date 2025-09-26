using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class GetPostIndexQuery : IQuery<OneOf<PostIndex, PostNotFoundError>>;

public sealed class GetPostIndexQueryHandler : IQueryHandler<GetPostIndexQuery, OneOf<PostIndex, PostNotFoundError>>
{
    private readonly IThreadReadRepository _repository;

    public GetPostIndexQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<PostIndex, PostNotFoundError>> HandleAsync(GetPostIndexQuery query,
        CancellationToken cancellationToken)
    {
        return _repository.GetPostIndexAsync(query.PostId, cancellationToken);
    }
}