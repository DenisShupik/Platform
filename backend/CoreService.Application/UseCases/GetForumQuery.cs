using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Generator.Attributes;
using OneOf;

namespace CoreService.Application.UseCases;

[IncludeAsRequired(typeof(Forum),nameof(Forum.ForumId))]
public sealed partial class GetForumQuery;

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