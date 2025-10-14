using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface ICategoryWriteRepository
{
    public Task<Result<CategoryThreadAddable, CategoryNotFoundError>> GetAsync(CategoryId categoryId,
        CancellationToken cancellationToken);
}