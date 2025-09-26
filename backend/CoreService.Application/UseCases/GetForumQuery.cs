using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using OneOf;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
public sealed partial class GetForumQuery<T> : IQuery<OneOf<T, ForumNotFoundError>>;

public sealed class GetForumQueryHandler<T> : IQueryHandler<GetForumQuery<T>, OneOf<T, ForumNotFoundError>>
{
    private readonly IForumReadRepository _repository;

    public GetForumQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<T, ForumNotFoundError>> HandleAsync(
        GetForumQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(query.ForumId, cancellationToken);
    }
}