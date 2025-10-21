using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IPolicyReadRepository
{
    public Task<Dictionary<PolicyId, Result<T, PolicyNotFoundError>>>
        GetBulkAsync<T>(
            GetPoliciesBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull;
}