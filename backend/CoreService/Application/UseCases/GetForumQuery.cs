using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class GetForumQuery
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required ForumId ForumId { get; init; }
}

public sealed class GetForumQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<OneOf<T, ForumNotFoundError>> HandleAsync<T>(
        GetForumQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(request.ForumId, cancellationToken);
    }

    public Task<OneOf<ForumDto, ForumNotFoundError>> HandleAsync(
        GetForumQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<ForumDto>(request, cancellationToken);
    }
}