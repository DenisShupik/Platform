using CoreService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCountQuery
{
    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
}

public sealed class GetForumsCountQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetForumsCountQueryResult> HandleAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCountAsync(request, cancellationToken);
    }
}