using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCountQuery : IQuery<Count>
{
    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
    
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class GetForumsCountQueryHandler : IQueryHandler<GetForumsCountQuery, Count>
{
    private readonly IForumReadRepository _repository;

    public GetForumsCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Count> HandleAsync(GetForumsCountQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetCountAsync(query, cancellationToken);
    }
}