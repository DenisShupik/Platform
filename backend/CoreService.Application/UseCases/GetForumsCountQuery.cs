using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCountQuery : IQuery<ulong>
{
    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
    
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetForumsCountQueryHandler : IQueryHandler<GetForumsCountQuery, ulong>
{
    private readonly IForumReadRepository _repository;

    public GetForumsCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<ulong> HandleAsync(GetForumsCountQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetCountAsync(query, cancellationToken);
    }
}