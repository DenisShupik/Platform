using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetPostsQuery : PaginatedQuery
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId? ThreadId { get; init; }
}

public sealed class GetPostsQueryValidator : PaginatedQueryValidator<GetPostsQuery>
{
}

public sealed class GetPostsQueryHandler
{
    private readonly IPostReadRepository _repository;

    public GetPostsQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetPostsQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<PostDto>> HandleAsync(GetPostsQuery request, CancellationToken cancellationToken)
    {
        return await HandleAsync<PostDto>(request, cancellationToken);
    }
}