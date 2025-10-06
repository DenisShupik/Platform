using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsCountQuery : IQuery<Dictionary<CategoryId, ulong>>
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }
    
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetCategoriesPostsCountQueryHandler : IQueryHandler<GetCategoriesPostsCountQuery, Dictionary<CategoryId, ulong>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<CategoryId, ulong>> HandleAsync(GetCategoriesPostsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetCategoriesPostsCountAsync(query, cancellationToken);
    }
}