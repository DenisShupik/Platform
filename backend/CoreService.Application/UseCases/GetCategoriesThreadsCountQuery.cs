using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesThreadsCountQuery : IQuery<Dictionary<CategoryId, ulong>>
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }

    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
    
    public required UserId? QueriedBy { get; init; }
}

public sealed class
    GetCategoriesThreadsCountQueryHandler : IQueryHandler<GetCategoriesThreadsCountQuery,
    Dictionary<CategoryId, ulong>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesThreadsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<CategoryId, ulong>> HandleAsync(GetCategoriesThreadsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetCategoriesThreadsCountAsync(query, cancellationToken);
    }
}