using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using GetThreadsCountQueryResult = Result<ulong, NotAdminError, NotOwnerError>;

public sealed class GetThreadsCountQuery : IQuery<GetThreadsCountQueryResult>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId? CreatedBy { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public required ThreadStatus? Status { get; init; }

    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetThreadsCountQueryHandler : IQueryHandler<GetThreadsCountQuery, GetThreadsCountQueryResult>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetThreadsCountQueryResult> HandleAsync(GetThreadsCountQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Status == ThreadStatus.Draft)
        {
            if (query.QueriedBy == null || query.CreatedBy == null) return new NotAdminError();

            if (query.CreatedBy != query.QueriedBy)
            {
                return new NotOwnerError();
            }
        }

        return await _repository.GetCountAsync(query, cancellationToken);
    }
}