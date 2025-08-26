using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface ICategoryWriteRepository
{
    public Task<OneOf<T, CategoryNotFoundError>> GetAsync<T>(CategoryId categoryId, CancellationToken cancellationToken)
        where T : class, IHasCategoryId;
}