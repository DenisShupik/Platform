using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface ICategoryWriteRepository
{
    public Task<Result<Category, CategoryNotFoundError>> GetAsync(CategoryId categoryId,
        CancellationToken cancellationToken);

    public void Add(Category category);
}