using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface ICategoryWriteRepository
{
    public Task<Result<T, CategoryNotFoundError>> GetAsync<T>(CategoryId categoryId, CancellationToken cancellationToken)
        where T : class, IHasCategoryId;
}