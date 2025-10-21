using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class
    GetPoliciesBulkQuery<T> : IQuery<Dictionary<PolicyId, Result<T, PolicyNotFoundError>>>
    where T : notnull
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<PolicyId, Guid> PolicyIds { get; init; }

    public required UserId? QueriedBy { get; init; }
}

public sealed class GetPoliciesBulkQueryHandler<T> : IQueryHandler<GetPoliciesBulkQuery<T>,
    Dictionary<PolicyId, Result<T, PolicyNotFoundError>>>
    where T : notnull
{
    private readonly IPolicyReadRepository _repository;

    public GetPoliciesBulkQueryHandler(IPolicyReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<PolicyId, Result<T, PolicyNotFoundError>>>
        HandleAsync(GetPoliciesBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync(query, cancellationToken);
}