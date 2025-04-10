using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface ICategoryReadRepository
{
    public Task<OneOf<T, CategoryNotFoundError>> GetByIdAsync<T>(CategoryId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<CategoryId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesQuery request, CancellationToken cancellationToken);
}