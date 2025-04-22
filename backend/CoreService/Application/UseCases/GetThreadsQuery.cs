using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsQuery : PaginatedQuery
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

public sealed class GetThreadsQueryValidator : PaginatedQueryValidator<GetThreadsQuery>
{
}

[GenerateOneOf]
public partial class GetThreadsQueryResult<T> : OneOfBase<List<T>, NotAdminError, NotOwnerError>;

public sealed class GetThreadsQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private async Task<GetThreadsQueryResult<T>> HandleAsync<T>(GetThreadsQuery query,
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

        return await _repository.GetAllAsync<T>(query, cancellationToken);
    }

    public Task<GetThreadsQueryResult<ThreadDto>> HandleAsync(GetThreadsQuery query,
        CancellationToken cancellationToken)
    {
        return HandleAsync<ThreadDto>(query, cancellationToken);
    }
}