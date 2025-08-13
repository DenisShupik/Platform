using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using OneOf;
using SharedKernel.Application.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPagedQuery : PagedQuery
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

public sealed class GetThreadsPagedQueryValidator : PagedQueryValidator<GetThreadsPagedQuery>;

[GenerateOneOf]
public partial class GetThreadsQueryResult<T> : OneOfBase<List<T>, NotAdminError, NotOwnerError>;

public sealed class GetThreadsPagedQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPagedQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private async Task<GetThreadsQueryResult<T>> HandleAsync<T>(GetThreadsPagedQuery pagedQuery,
        CancellationToken cancellationToken)
    {
        if (pagedQuery.Status == ThreadStatus.Draft)
        {
            if (pagedQuery.QueriedBy == null || pagedQuery.CreatedBy == null) return new NotAdminError();

            if (pagedQuery.CreatedBy != pagedQuery.QueriedBy)
            {
                return new NotOwnerError();
            }
        }

        return await _repository.GetAllAsync<T>(pagedQuery, cancellationToken);
    }

    public Task<GetThreadsQueryResult<ThreadDto>> HandleAsync(GetThreadsPagedQuery pagedQuery,
        CancellationToken cancellationToken)
    {
        return HandleAsync<ThreadDto>(pagedQuery, cancellationToken);
    }
}