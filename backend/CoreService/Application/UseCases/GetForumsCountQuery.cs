using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCountQuery
{
    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
    
    public required ForumContainsFilter? Contains { get; init; }
}

public sealed class GetForumsCountQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> HandleAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCountAsync(request, cancellationToken);
    }
}